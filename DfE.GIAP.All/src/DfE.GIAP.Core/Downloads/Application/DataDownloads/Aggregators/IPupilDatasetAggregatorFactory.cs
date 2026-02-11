using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators;

public interface IPupilDatasetAggregatorFactory
{
    Task<PupilDatasetCollection> AggregateAsync(
        DownloadType downloadType,
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets);
}
