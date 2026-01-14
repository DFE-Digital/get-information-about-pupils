using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators;

public interface IPupilDatasetAggregator
{
    Task<PupilDatasetCollection> AggregateAsync(
        DownloadType downloadType,
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets);
}
