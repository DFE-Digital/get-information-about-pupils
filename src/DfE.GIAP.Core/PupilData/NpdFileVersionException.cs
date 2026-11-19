using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

namespace DfE.GIAP.Core.PupilData;

public sealed class NpdFileVersionException(
    NpdDataset dataset,
    string academicYear,
    IngestVersion ingestVersion,
    IngestVersion storedIngestVersion)
    : Exception(
        $"The {dataset} {academicYear} file is {ingestVersion} but the stored data is from a more mature " +
        $"{storedIngestVersion} file.")
{
    public NpdDataset Dataset { get; } = dataset;
    public string AcademicYear { get; } = academicYear;
    public IngestVersion IngestVersion { get; } = ingestVersion;
    public IngestVersion StoredIngestVersion { get; } = storedIngestVersion;
}
