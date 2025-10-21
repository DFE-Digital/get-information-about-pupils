using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.DatasetCheckers;

/// <summary>
/// Provides supported datasets for each download type.
/// </summary>
public static class DownloadDatasetMap
{
    private static readonly Dictionary<DownloadType, HashSet<Datasets>> _map = new()
    {
        [DownloadType.FurtherEducation] = new()
        {
            Datasets.PupilPremium,
            Datasets.SEN
        },
        [DownloadType.PupilPremium] = new()
        {
            Datasets.PupilPremium
        },
        [DownloadType.NPD] = new()
        {
            Datasets.CensusAutumn,
            Datasets.CensusSpring,
            Datasets.CensusSummer,
            Datasets.EYFSP,
            Datasets.KS1,
            Datasets.KS2,
            Datasets.KS4,
            Datasets.Phonics,
            Datasets.MTC
        }
    };

    /// <summary>
    /// Returns the list of datasets supported for a given download type.
    /// </summary>
    public static IReadOnlyCollection<Datasets> GetSupportedDatasets(DownloadType type)
    {
        return _map.TryGetValue(type, out HashSet<Datasets>? datasets)
            ? datasets
            : Array.Empty<Datasets>();
    }

    /// <summary>
    /// Checks if a specific dataset is supported for a given download type.
    /// </summary>
    public static bool IsSupported(DownloadType type, Datasets dataset)
    {
        return _map.TryGetValue(type, out HashSet<Datasets>? datasets) && datasets.Contains(dataset);
    }
}
