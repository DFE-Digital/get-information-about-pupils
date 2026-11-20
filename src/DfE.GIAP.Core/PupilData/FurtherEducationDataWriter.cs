using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;

namespace DfE.GIAP.Core.PupilData;

public class FurtherEducationDataWriter(IDbContextFactory<GiapDbContext> dbContextFactory)
{
    // Key for the advisory lock that stops two FE files from being written at once ('fe' in ASCII)
    private const long AdvisoryLockKey = 0x6665;

    /// <summary>
    /// Writes the rows from a further education file, then updates the affected <see cref="FurtherEducationLearner"/>s.
    /// </summary>
    /// <remarks>
    /// A learner's rows are combined into one record whose <see cref="FurtherEducationLearnerRecord.Data"/> is an array
    /// of the rows in file order, and whose identifying fields come from the first row.
    /// A pupil premium file replaces all FE data, including SEN, as the old pipeline emptied the container before each
    /// one. A SEN file replaces the SEN records for its academic year; learners that have one but aren't in the file
    /// lose it (the old pipeline kept them). Either way, a learner left with no records is removed.
    /// </remarks>
    /// <param name="academicYear">The academic year from the file's name, e.g. <c>2024/2025</c>.</param>
    /// <param name="rows">Streamed into the database, so the file is never held in memory.</param>
    public async Task<FurtherEducationFileWriteResult> WriteFileAsync(
        FurtherEducationDataset dataset,
        string academicYear,
        IAsyncEnumerable<FurtherEducationFileRow> rows,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(academicYear);
        ArgumentNullException.ThrowIfNull(rows);

        string datasetName = dataset.ToString();

        await using GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({AdvisoryLockKey})", cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TEMP TABLE further_education_file_rows (
                row_number integer NOT NULL,
                uln text NOT NULL,
                surname text,
                forename text,
                sex text,
                dob date,
                data jsonb NOT NULL
            ) ON COMMIT DROP
            """,
            cancellationToken);

        if (await CopyRowsAsync(dbContext, rows, cancellationToken) == 0)
        {
            throw new ArgumentException("There must be at least one row.", nameof(rows));
        }

        // Temp tables aren't analyzed automatically, and the planner needs their statistics for the grouping below
        await dbContext.Database.ExecuteSqlRawAsync("ANALYZE further_education_file_rows", cancellationToken);

        // INCLUDING DEFAULTS so that created_at is set to the transaction's start time
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE TEMP TABLE further_education_file_staging (LIKE further_education_learner_records INCLUDING DEFAULTS) ON COMMIT DROP",
            cancellationToken);
        await dbContext.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO further_education_file_staging (uln, dataset, academic_year, surname, forename, sex, dob, data)
            SELECT DISTINCT ON (f.uln)
                f.uln, {datasetName}, {academicYear}, f.surname, f.forename, f.sex, f.dob, d.data
            FROM further_education_file_rows f
            JOIN (
                SELECT uln, jsonb_agg(data ORDER BY row_number) AS data
                FROM further_education_file_rows
                GROUP BY uln
            ) d ON d.uln = f.uln
            ORDER BY f.uln, f.row_number
            """,
            cancellationToken);

        // The learners in the file, and the learners that will lose records
        if (dataset == FurtherEducationDataset.PupilPremium)
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TEMP TABLE further_education_affected_learners ON COMMIT DROP AS
                SELECT uln FROM further_education_file_staging
                UNION
                SELECT uln FROM further_education_learners
                """,
                cancellationToken);
        }
        else
        {
            await dbContext.Database.ExecuteSqlAsync(
                $"""
                CREATE TEMP TABLE further_education_affected_learners ON COMMIT DROP AS
                SELECT uln FROM further_education_file_staging
                UNION
                SELECT uln FROM further_education_learner_records WHERE dataset = {datasetName} AND academic_year = {academicYear}
                """,
                cancellationToken);
        }

        int recordsRemoved = dataset == FurtherEducationDataset.PupilPremium
            ? await dbContext.Database.ExecuteSqlRawAsync(
                """
                DELETE FROM further_education_learner_records r
                WHERE NOT EXISTS (
                    SELECT 1 FROM further_education_file_staging s
                    WHERE s.uln = r.uln AND s.dataset = r.dataset AND s.academic_year = r.academic_year)
                """,
                cancellationToken)
            : await dbContext.Database.ExecuteSqlAsync(
                $"""
                DELETE FROM further_education_learner_records r
                WHERE r.dataset = {datasetName} AND r.academic_year = {academicYear}
                AND NOT EXISTS (SELECT 1 FROM further_education_file_staging s WHERE s.uln = r.uln)
                """,
                cancellationToken);

        // Placeholders to satisfy the foreign key; they're filled in below
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO further_education_learners (uln)
            SELECT DISTINCT uln FROM further_education_file_staging
            ON CONFLICT DO NOTHING
            """,
            cancellationToken);

        // FE files have no maturity, so a record always keeps its created_at when it's replaced
        int recordsWritten = await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO further_education_learner_records
            SELECT * FROM further_education_file_staging
            ON CONFLICT (uln, dataset, academic_year) DO UPDATE SET
                surname = excluded.surname,
                forename = excluded.forename,
                sex = excluded.sex,
                dob = excluded.dob,
                data = excluded.data
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM further_education_learners l
            USING further_education_affected_learners a
            WHERE l.uln = a.uln
            AND NOT EXISTS (SELECT 1 FROM further_education_learner_records r WHERE r.uln = l.uln)
            """,
            cancellationToken);

        // Take the identifying fields from a pupil premium record if there is one, then the latest academic year, then
        // the record loaded first; dataset is only there to make the order deterministic
        await dbContext.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO further_education_learners (uln, surname, forename, sex, dob)
            SELECT DISTINCT ON (r.uln)
                r.uln, r.surname, r.forename, r.sex, r.dob
            FROM further_education_learner_records r
            JOIN further_education_affected_learners a ON a.uln = r.uln
            ORDER BY r.uln, r.dataset = {nameof(FurtherEducationDataset.PupilPremium)} DESC, r.academic_year DESC, r.created_at, r.dataset
            ON CONFLICT (uln) DO UPDATE SET
                surname = excluded.surname,
                forename = excluded.forename,
                sex = excluded.sex,
                dob = excluded.dob
            """,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new FurtherEducationFileWriteResult(RecordsWritten: recordsWritten, RecordsRemoved: recordsRemoved);
    }

    private static async Task<int> CopyRowsAsync(
        GiapDbContext dbContext,
        IAsyncEnumerable<FurtherEducationFileRow> rows,
        CancellationToken cancellationToken)
    {
        NpgsqlConnection connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        int count = 0;

        await using NpgsqlBinaryImporter importer = await connection.BeginBinaryImportAsync(
            "COPY further_education_file_rows (row_number, uln, surname, forename, sex, dob, data) FROM STDIN (FORMAT BINARY)",
            cancellationToken);

        await foreach (FurtherEducationFileRow row in rows.WithCancellation(cancellationToken))
        {
            await importer.StartRowAsync(cancellationToken);
            await importer.WriteAsync(row.RowNumber, NpgsqlDbType.Integer, cancellationToken);
            await importer.WriteAsync(row.Uln, NpgsqlDbType.Text, cancellationToken);
            await WriteNullableAsync(row.Surname, NpgsqlDbType.Text);
            await WriteNullableAsync(row.Forename, NpgsqlDbType.Text);
            await WriteNullableAsync(row.Sex, NpgsqlDbType.Text);
            await WriteNullableAsync(row.Dob, NpgsqlDbType.Date);
            await importer.WriteAsync(row.Data, NpgsqlDbType.Jsonb, cancellationToken);
            count++;
        }

        await importer.CompleteAsync(cancellationToken);

        return count;

        async Task WriteNullableAsync<T>(T? value, NpgsqlDbType type)
        {
            if (value is null)
            {
                await importer.WriteNullAsync(cancellationToken);
            }
            else
            {
                await importer.WriteAsync(value, type, cancellationToken);
            }
        }
    }
}

public sealed record FurtherEducationFileWriteResult(int RecordsWritten, int RecordsRemoved);
