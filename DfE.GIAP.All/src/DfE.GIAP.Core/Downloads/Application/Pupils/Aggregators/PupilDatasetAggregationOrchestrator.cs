using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators.Handlers;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators;

public class PupilDatasetAggregationOrchestrator : IPupilDatasetAggregator
{
    private readonly IReadOnlyList<IPupilDatasetAggregationHandler> _strategies;

    public PupilDatasetAggregationOrchestrator(IEnumerable<IPupilDatasetAggregationHandler> strategies)
        => _strategies = strategies.ToList();

    public Task<PupilDatasetCollection> AggregateAsync(
        DownloadType downloadType,
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets)
    {
        IPupilDatasetAggregationHandler? strategy = _strategies.FirstOrDefault(s => s.CanHandle(downloadType));
        if (strategy is null)
            throw new NotSupportedException($"No strategy registered for {downloadType}");

        return strategy.AggregateAsync(pupilIds, selectedDatasets);
    }
}
