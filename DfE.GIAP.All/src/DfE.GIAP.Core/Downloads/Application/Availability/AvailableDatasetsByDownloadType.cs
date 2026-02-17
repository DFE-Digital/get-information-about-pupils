using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Availability;

/// <summary>
/// Provides supported datasets for each download type.
/// </summary>
public static class AvailableDatasetsByDownloadType
{
    private static readonly Dictionary<PupilDownloadType, HashSet<Dataset>> _map = new()
    {
        [PupilDownloadType.FurtherEducation] = new()
        {
            Dataset.FE_PP,
            Dataset.SEN
        },
        [PupilDownloadType.PupilPremium] = new()
        {
            Dataset.PP
        },
        [PupilDownloadType.NPD] = new()
        {
            Dataset.Census_Autumn,
            Dataset.Census_Spring,
            Dataset.Census_Summer,
            Dataset.EYFSP,
            Dataset.KS1,
            Dataset.KS2,
            Dataset.KS4,
            Dataset.Phonics,
            Dataset.MTC
        }
    };

    /// <summary>
    /// Returns the list of datasets supported for a given download type.
    /// </summary>
    public static IReadOnlyCollection<Dataset> GetSupportedDatasets(PupilDownloadType type)
    {
        return _map.TryGetValue(type, out HashSet<Dataset>? datasets)
            ? datasets
            : Array.Empty<Dataset>();
    }
}
