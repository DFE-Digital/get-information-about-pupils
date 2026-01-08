using DfE.GIAP.Core.Downloads.Application.Datasets.Availability;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

internal class DatasetAvailabilityHandlerFactoryTestDouble
{
    public static IDatasetAvailabilityHandlerFactory WithHandlers(params IDatasetAvailabilityHandler[] handlers) =>
        new StubFactory(handlers.ToDictionary(h => h.SupportedDownloadType));

    public static IDatasetAvailabilityHandlerFactory WithHandler(DownloadType type, IDatasetAvailabilityHandler handler) =>
        new StubFactory(new Dictionary<DownloadType, IDatasetAvailabilityHandler> { [type] = handler });

    private sealed class StubFactory : IDatasetAvailabilityHandlerFactory
    {
        private readonly IDictionary<DownloadType, IDatasetAvailabilityHandler> _handlers;

        public StubFactory(IDictionary<DownloadType, IDatasetAvailabilityHandler> handlers)
        {
            _handlers = handlers;
        }

        public IDatasetAvailabilityHandler GetDatasetAvailabilityHandler(DownloadType type)
        {
            if (_handlers.TryGetValue(type, out IDatasetAvailabilityHandler? handler))
                return handler;

            throw new NotSupportedException($"No handler registered for DownloadType '{type}'");
        }
    }
}
