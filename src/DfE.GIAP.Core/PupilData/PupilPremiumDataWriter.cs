using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;

namespace DfE.GIAP.Core.PupilData;

public class PupilPremiumDataWriter(IDbContextFactory<GiapDbContext> dbContextFactory)
{
    // Key for the advisory lock that stops two pupil premium files from being written at once ('pp' in ASCII)
    private const long AdvisoryLockKey = 0x7070;

    /// <summary>
    /// Writes the rows from a pupil premium file, replacing all the <see cref="PupilPremiumPupil"/>s.
    /// </summary>
    /// <remarks>
    /// A pupil's rows are combined into one pupil whose <see cref="PupilPremiumPupil.Data"/> is an array of the rows in
    /// file order, and whose identifying fields come from the first row. Pupils that aren't in the file are removed,
    /// as the old pipeline emptied the container before each file.
    /// </remarks>
    /// <param name="academicYear">The academic year from the file's name, e.g. <c>2024/2025</c>.</param>
    /// <param name="rows">Streamed into the database, so the file is never held in memory.</param>
    /// <exception cref="PupilPremiumFileVersionException">
    /// The stored data is from a more recent file: a later academic year, or the same year and a more mature file.
    /// Nothing is changed.
    /// </exception>
    public async Task<PupilPremiumFileWriteResult> WriteFileAsync(
        string academicYear,
        IngestVersion ingestVersion,
        IAsyncEnumerable<PupilPremiumFileRow> rows,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(academicYear);
        ArgumentNullException.ThrowIfNull(rows);

        await using GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({AdvisoryLockKey})", cancellationToken);

        // The stored data is from the last file that succeeded, since each file replaces all of it
        var stored = await dbContext.IngestRuns
            .Where(r => r.Dataset == IngestDataset.PupilPremium && r.Status == IngestRunStatus.Succeeded)
            .OrderByDescending(r => r.FinishedAt)
            .Select(r => new { r.AcademicYear, r.IngestVersion })
            .FirstOrDefaultAsync(cancellationToken);

        // Academic years are yyyy/yyyy, so they sort as strings
        if (stored is { AcademicYear: { } storedAcademicYear, IngestVersion: { } storedIngestVersion }
            && (string.CompareOrdinal(storedAcademicYear, academicYear) > 0
                || (storedAcademicYear == academicYear && storedIngestVersion > ingestVersion)))
        {
            throw new PupilPremiumFileVersionException(academicYear, ingestVersion, storedAcademicYear, storedIngestVersion);
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TEMP TABLE pupil_premium_file_rows (
                row_number integer NOT NULL,
                pupil_matching_ref text NOT NULL,
                upn text,
                la_code integer,
                establishment_number integer,
                urn integer,
                surname text,
                forename text,
                middle_names text,
                sex text,
                dob date,
                source text,
                data jsonb NOT NULL
            ) ON COMMIT DROP
            """,
            cancellationToken);

        if (await CopyRowsAsync(dbContext, rows, cancellationToken) == 0)
        {
            throw new ArgumentException("There must be at least one row.", nameof(rows));
        }

        // Temp tables aren't analyzed automatically, and the planner needs their statistics for the grouping below
        await dbContext.Database.ExecuteSqlRawAsync("ANALYZE pupil_premium_file_rows", cancellationToken);

        int recordsRemoved = await dbContext.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM pupil_premium_pupils p
            WHERE NOT EXISTS (SELECT 1 FROM pupil_premium_file_rows f WHERE f.pupil_matching_ref = p.pupil_matching_ref)
            """,
            cancellationToken);

        // source_flag is the old pipeline's Source_flag: Both if the pupil's rows have more than one Source
        int recordsWritten = await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO pupil_premium_pupils (pupil_matching_ref, upn, la_code, establishment_number, urn, surname, forename, middle_names, sex, dob, source_flag, data)
            SELECT DISTINCT ON (f.pupil_matching_ref)
                f.pupil_matching_ref, f.upn, f.la_code, f.establishment_number, f.urn, f.surname, f.forename, f.middle_names, f.sex, f.dob, d.source_flag, d.data
            FROM pupil_premium_file_rows f
            JOIN (
                SELECT
                    pupil_matching_ref,
                    CASE WHEN count(DISTINCT source) > 1 THEN 'Both' ELSE min(source) END AS source_flag,
                    jsonb_agg(data ORDER BY row_number) AS data
                FROM pupil_premium_file_rows
                GROUP BY pupil_matching_ref
            ) d ON d.pupil_matching_ref = f.pupil_matching_ref
            ORDER BY f.pupil_matching_ref, f.row_number
            ON CONFLICT (pupil_matching_ref) DO UPDATE SET
                upn = excluded.upn,
                la_code = excluded.la_code,
                establishment_number = excluded.establishment_number,
                urn = excluded.urn,
                surname = excluded.surname,
                forename = excluded.forename,
                middle_names = excluded.middle_names,
                sex = excluded.sex,
                dob = excluded.dob,
                source_flag = excluded.source_flag,
                data = excluded.data
            """,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new PupilPremiumFileWriteResult(RecordsWritten: recordsWritten, RecordsRemoved: recordsRemoved);
    }

    private static async Task<int> CopyRowsAsync(
        GiapDbContext dbContext,
        IAsyncEnumerable<PupilPremiumFileRow> rows,
        CancellationToken cancellationToken)
    {
        NpgsqlConnection connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        int count = 0;

        await using NpgsqlBinaryImporter importer = await connection.BeginBinaryImportAsync(
            "COPY pupil_premium_file_rows (row_number, pupil_matching_ref, upn, la_code, establishment_number, urn, surname, forename, middle_names, sex, dob, source, data) FROM STDIN (FORMAT BINARY)",
            cancellationToken);

        await foreach (PupilPremiumFileRow row in rows.WithCancellation(cancellationToken))
        {
            await importer.StartRowAsync(cancellationToken);
            await importer.WriteAsync(row.RowNumber, NpgsqlDbType.Integer, cancellationToken);
            await importer.WriteAsync(row.PupilMatchingRef, NpgsqlDbType.Text, cancellationToken);
            await WriteNullableAsync(row.Upn, NpgsqlDbType.Text);
            await WriteNullableAsync(row.LaCode, NpgsqlDbType.Integer);
            await WriteNullableAsync(row.EstablishmentNumber, NpgsqlDbType.Integer);
            await WriteNullableAsync(row.Urn, NpgsqlDbType.Integer);
            await WriteNullableAsync(row.Surname, NpgsqlDbType.Text);
            await WriteNullableAsync(row.Forename, NpgsqlDbType.Text);
            await WriteNullableAsync(row.MiddleNames, NpgsqlDbType.Text);
            await WriteNullableAsync(row.Sex, NpgsqlDbType.Text);
            await WriteNullableAsync(row.Dob, NpgsqlDbType.Date);
            await WriteNullableAsync(row.Source, NpgsqlDbType.Text);
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

public sealed record PupilPremiumFileWriteResult(int RecordsWritten, int RecordsRemoved);
