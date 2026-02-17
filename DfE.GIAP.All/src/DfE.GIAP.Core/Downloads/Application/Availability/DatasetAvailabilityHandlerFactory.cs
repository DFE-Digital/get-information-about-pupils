using DfE.GIAP.Core.Downloads.Application.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Availability;

public class DatasetAvailabilityHandlerFactory : IDatasetAvailabilityHandlerFactory
{
    private readonly IDictionary<PupilDownloadType, IDatasetAvailabilityHandler> _handlers;

    public DatasetAvailabilityHandlerFactory(IEnumerable<IDatasetAvailabilityHandler> checkers)
    {
        // Build a dictionary from the available handlers
        _handlers = checkers.ToDictionary(
            checker => checker.SupportedDownloadType,
            checker => checker);
    }

    public IDatasetAvailabilityHandler GetDatasetAvailabilityHandler(PupilDownloadType type)
    {
        if (_handlers.TryGetValue(type, out IDatasetAvailabilityHandler? handler))
            return handler;

        throw new NotSupportedException($"No dataset availability handler registered for DownloadType '{type}'");
    }
}
