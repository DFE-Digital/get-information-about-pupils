using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators.Handlers;

public interface IPupilDatasetAggregationHandler
{
    bool CanHandle(DownloadType downloadType);

    Task<PupilDatasetCollection> AggregateAsync(
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets,
        CancellationToken cancellationToken = default);
}
