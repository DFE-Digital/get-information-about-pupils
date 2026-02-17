using DfE.GIAP.Core.Downloads.Application.Availability;
using DfE.GIAP.Core.Downloads.Application.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

internal class DatasetAvailabilityHandlerFactoryTestDouble
{
    public static IDatasetAvailabilityHandlerFactory WithHandlers(params IDatasetAvailabilityHandler[] handlers) =>
        new StubFactory(handlers.ToDictionary(h => h.SupportedDownloadType));

    public static IDatasetAvailabilityHandlerFactory WithHandler(PupilDownloadType type, IDatasetAvailabilityHandler handler) =>
        new StubFactory(new Dictionary<PupilDownloadType, IDatasetAvailabilityHandler> { [type] = handler });

    private sealed class StubFactory : IDatasetAvailabilityHandlerFactory
    {
        private readonly IDictionary<PupilDownloadType, IDatasetAvailabilityHandler> _handlers;

        public StubFactory(IDictionary<PupilDownloadType, IDatasetAvailabilityHandler> handlers)
        {
            _handlers = handlers;
        }

        public IDatasetAvailabilityHandler GetDatasetAvailabilityHandler(PupilDownloadType type)
        {
            if (_handlers.TryGetValue(type, out IDatasetAvailabilityHandler? handler))
                return handler;

            throw new NotSupportedException($"No handler registered for DownloadType '{type}'");
        }
    }
}
