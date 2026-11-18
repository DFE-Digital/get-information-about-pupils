using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DfE.GIAP.Core.PupilData;

public class NpdDataWriter(IDbContextFactory<GiapDbContext> dbContextFactory)
{
    // Key for the advisory lock that stops two NPD files from being written at once ('npd' in ASCII)
    private const long AdvisoryLockKey = 0x6e7064;

    private static readonly string[] CensusDatasets =
        [nameof(NpdDataset.CensusAutumn), nameof(NpdDataset.CensusSpring), nameof(NpdDataset.CensusSummer)];

    /// <summary>
    /// Writes the records from an NPD file, replacing all the records for its dataset and academic year,
    /// then updates the affected <see cref="NpdPupil"/>s.
    /// </summary>
    /// <remarks>
    /// This matches the old pipeline: pupils that have a record for the dataset and year but aren't in the file lose
    /// that record, and a pupil left with no records is removed.
    /// </remarks>
    /// <param name="academicYear">The academic year from the file's name, e.g. <c>2024/2025</c>.</param>
    /// <exception cref="NpdFileVersionException">
    /// The records already stored for the dataset and academic year are from a more mature file. Nothing is changed.
    /// </exception>
    public async Task<NpdFileWriteResult> WriteFileAsync(
        NpdDataset dataset,
        string academicYear,
        IngestVersion ingestVersion,
        IReadOnlyCollection<NpdPupilRecord> records,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(academicYear);
        ArgumentNullException.ThrowIfNull(records);

        if (records.Count == 0)
        {
            throw new ArgumentException("There must be at least one record.", nameof(records));
        }

        if (records.Any(r => r.Dataset != dataset || r.IngestVersion != ingestVersion))
        {
            throw new ArgumentException("Every record must have the file's dataset and ingest version.", nameof(records));
        }

        string datasetName = dataset.ToString();

        await using GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({AdvisoryLockKey})", cancellationToken);

        IngestVersion? storedVersion = await dbContext.NpdPupilRecords
            .Where(r => r.Dataset == dataset && r.AcademicYear == academicYear)
            .MaxAsync(r => (IngestVersion?)r.IngestVersion, cancellationToken);

        if (storedVersion > ingestVersion)
        {
            throw new NpdFileVersionException(dataset, academicYear, ingestVersion, storedVersion.Value);
        }

        // INCLUDING DEFAULTS so that created_at is set to the transaction's start time
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE TEMP TABLE npd_file_staging (LIKE npd_pupil_records INCLUDING DEFAULTS) ON COMMIT DROP",
            cancellationToken);
        await BinaryCopy.CopyAsync(dbContext, records, "npd_file_staging", cancellationToken);

        // The pupils in the file, and the pupils that will lose their record for this dataset and year
        await dbContext.Database.ExecuteSqlAsync(
            $"""
            CREATE TEMP TABLE npd_affected_pupils ON COMMIT DROP AS
            SELECT pupil_matching_ref FROM npd_file_staging
            UNION
            SELECT pupil_matching_ref FROM npd_pupil_records WHERE dataset = {datasetName} AND academic_year = {academicYear}
            """,
            cancellationToken);

        int recordsRemoved = await dbContext.Database.ExecuteSqlAsync(
            $"""
            DELETE FROM npd_pupil_records r
            WHERE r.dataset = {datasetName} AND r.academic_year = {academicYear}
            AND NOT EXISTS (SELECT 1 FROM npd_file_staging s WHERE s.pupil_matching_ref = r.pupil_matching_ref)
            """,
            cancellationToken);

        // Placeholders to satisfy the foreign key; they're filled in below
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO npd_pupils (pupil_matching_ref)
            SELECT DISTINCT pupil_matching_ref FROM npd_file_staging
            ON CONFLICT DO NOTHING
            """,
            cancellationToken);

        // A record keeps its created_at when it's replaced by the same version (i.e. the file is reprocessed).
        // The WHERE only matters for a record whose academic year differs from the file's; the version check above
        // covers the rest.
        int recordsWritten = await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO npd_pupil_records
            SELECT * FROM npd_file_staging
            ON CONFLICT (pupil_matching_ref, dataset, academic_year) DO UPDATE SET
                ingest_version = excluded.ingest_version,
                upn = excluded.upn,
                la_code = excluded.la_code,
                establishment_number = excluded.establishment_number,
                urn = excluded.urn,
                surname = excluded.surname,
                forename = excluded.forename,
                middle_names = excluded.middle_names,
                sex = excluded.sex,
                dob = excluded.dob,
                data = excluded.data,
                created_at = CASE WHEN npd_pupil_records.ingest_version = excluded.ingest_version
                    THEN npd_pupil_records.created_at ELSE excluded.created_at END
            WHERE excluded.ingest_version >= npd_pupil_records.ingest_version
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM npd_pupils p
            USING npd_affected_pupils a
            WHERE p.pupil_matching_ref = a.pupil_matching_ref
            AND NOT EXISTS (SELECT 1 FROM npd_pupil_records r WHERE r.pupil_matching_ref = p.pupil_matching_ref)
            """,
            cancellationToken);

        // Take the identifying fields from a census record if there is one, then the latest academic year, then the
        // record loaded first; dataset is only there to make the order deterministic
        await dbContext.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO npd_pupils (pupil_matching_ref, upn, la_code, establishment_number, urn, surname, forename, middle_names, sex, dob)
            SELECT DISTINCT ON (r.pupil_matching_ref)
                r.pupil_matching_ref, r.upn, r.la_code, r.establishment_number, r.urn, r.surname, r.forename, r.middle_names, r.sex, r.dob
            FROM npd_pupil_records r
            JOIN npd_affected_pupils a ON a.pupil_matching_ref = r.pupil_matching_ref
            ORDER BY r.pupil_matching_ref, r.dataset = ANY({CensusDatasets}) DESC, r.academic_year DESC, r.created_at, r.dataset
            ON CONFLICT (pupil_matching_ref) DO UPDATE SET
                upn = excluded.upn,
                la_code = excluded.la_code,
                establishment_number = excluded.establishment_number,
                urn = excluded.urn,
                surname = excluded.surname,
                forename = excluded.forename,
                middle_names = excluded.middle_names,
                sex = excluded.sex,
                dob = excluded.dob
            """,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new NpdFileWriteResult(RecordsWritten: recordsWritten, RecordsRemoved: recordsRemoved);
    }
}

public sealed record NpdFileWriteResult(int RecordsWritten, int RecordsRemoved);
