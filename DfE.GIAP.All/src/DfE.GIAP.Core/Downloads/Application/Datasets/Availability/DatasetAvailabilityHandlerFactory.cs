using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Availability;

public class DatasetAvailabilityHandlerFactory : IDatasetAvailabilityProviderFactory
{
    private readonly IDictionary<DownloadType, IDatasetAvailabilityHandler> _handlers;

    public DatasetAvailabilityHandlerFactory(IEnumerable<IDatasetAvailabilityHandler> checkers)
    {
        // Build a dictionary from the available checkers
        _handlers = checkers.ToDictionary(
            checker => checker.SupportedDownloadType,
            checker => checker);
    }

    public IDatasetAvailabilityHandler GetDatasetAvailabilityHandler(DownloadType type)
    {
        if (_handlers.TryGetValue(type, out IDatasetAvailabilityHandler? handler))
            return handler;

        throw new NotSupportedException($"No dataset availability handler registered for DownloadType '{type}'");
    }
}
