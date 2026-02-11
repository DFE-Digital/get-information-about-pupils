using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Availability;

/// <summary>
/// Provides supported datasets for each download type.
/// </summary>
public static class AvailableDatasetsByDownloadType
{
    private static readonly Dictionary<DownloadType, HashSet<Dataset>> _map = new()
    {
        [DownloadType.FurtherEducation] = new()
        {
            Dataset.FE_PP,
            Dataset.SEN
        },
        [DownloadType.PupilPremium] = new()
        {
            Dataset.PP
        },
        [DownloadType.NPD] = new()
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
    public static IReadOnlyCollection<Dataset> GetSupportedDatasets(DownloadType type)
    {
        return _map.TryGetValue(type, out HashSet<Dataset>? datasets)
            ? datasets
            : Array.Empty<Dataset>();
    }
}
