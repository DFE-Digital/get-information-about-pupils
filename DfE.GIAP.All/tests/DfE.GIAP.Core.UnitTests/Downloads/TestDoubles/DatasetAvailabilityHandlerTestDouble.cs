using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class DatasetAvailabilityHandlerTestDouble
{
    public static IDatasetAvailabilityHandler Create(DownloadType type) => new StubHandler(type);

    private sealed class StubHandler : IDatasetAvailabilityHandler
    {
        public DownloadType SupportedDownloadType { get; }

        public StubHandler(DownloadType type)
        {
            SupportedDownloadType = type;
        }

        public Task<IEnumerable<Dataset>> GetAvailableDatasetsAsync(IEnumerable<string> pupilIds)
        {
            return Task.FromResult<IEnumerable<Dataset>>(new[] { Dataset.PP });
        }
    }
}
