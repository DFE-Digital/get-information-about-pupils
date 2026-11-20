using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

namespace DfE.GIAP.Core.Ingest;

public sealed class IngestDatasetProcessorRegistry
{
    private readonly Dictionary<IngestDataset, IIngestDatasetProcessor> _processors = [];

    public IngestDatasetProcessorRegistry(IEnumerable<IIngestDatasetProcessor> processors)
    {
        foreach (IIngestDatasetProcessor processor in processors)
        {
            foreach (IngestDataset dataset in processor.Datasets)
            {
                if (!_processors.TryAdd(dataset, processor))
                {
                    throw new InvalidOperationException($"More than one processor is registered for {dataset}.");
                }
            }
        }
    }

    public IIngestDatasetProcessor? GetProcessor(IngestDataset dataset) => _processors.GetValueOrDefault(dataset);
}
