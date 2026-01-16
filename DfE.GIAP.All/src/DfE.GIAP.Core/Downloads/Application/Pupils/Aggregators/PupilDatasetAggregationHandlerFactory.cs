using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators.Handlers;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators;

public class PupilDatasetAggregationHandlerFactory : IPupilDatasetAggregatorFactory
{
    private readonly IReadOnlyList<IPupilDatasetAggregationHandler> _handlers;

    public PupilDatasetAggregationHandlerFactory(IEnumerable<IPupilDatasetAggregationHandler> handlers)
        => _handlers = handlers.ToList();

    public Task<PupilDatasetCollection> AggregateAsync(
        DownloadType downloadType,
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets)
    {
        IPupilDatasetAggregationHandler? handler = _handlers.FirstOrDefault(s => s.CanHandle(downloadType));
        if (handler is null)
            throw new NotSupportedException($"No handler registered for {downloadType}");

        return handler.AggregateAsync(pupilIds, selectedDatasets);
    }
}
