using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators;

public class PupilDatasetAggregationHandlerFactory : IPupilDatasetAggregatorFactory
{
    private readonly Dictionary<DownloadType, IPupilDatasetAggregationHandler> _handlers;

    public PupilDatasetAggregationHandlerFactory(IEnumerable<IPupilDatasetAggregationHandler> handlers)
    {
        _handlers = handlers.ToDictionary(
           handler => handler.SupportedDownloadType,
           handler => handler);
    }

    public Task<PupilDatasetCollection> AggregateAsync(
        DownloadType downloadType,
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets)
    {
        if (_handlers.TryGetValue(downloadType, out IPupilDatasetAggregationHandler? handler))
            return handler.AggregateAsync(pupilIds, selectedDatasets);

        throw new NotSupportedException($"No pupil aggregator handler registered for DownloadType '{downloadType}'");
    }
}
