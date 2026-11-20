using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

namespace DfE.GIAP.Core.PupilData;

public sealed class PupilPremiumFileVersionException(
    string academicYear,
    IngestVersion ingestVersion,
    string storedAcademicYear,
    IngestVersion storedIngestVersion)
    : Exception(
        $"The pupil premium file is {academicYear} {ingestVersion} but the stored data is from a more recent " +
        $"{storedAcademicYear} {storedIngestVersion} file.")
{
    public string AcademicYear { get; } = academicYear;
    public IngestVersion IngestVersion { get; } = ingestVersion;
    public string StoredAcademicYear { get; } = storedAcademicYear;
    public IngestVersion StoredIngestVersion { get; } = storedIngestVersion;
}
