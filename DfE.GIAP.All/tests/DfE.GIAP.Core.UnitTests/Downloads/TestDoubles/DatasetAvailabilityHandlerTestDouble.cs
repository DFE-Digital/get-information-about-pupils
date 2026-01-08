using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class DatasetAvailabilityHandlerTestDouble
{
    public static IDatasetAvailabilityHandler Create(
        DownloadType type,
        IEnumerable<Dataset>? datasetsWithData = null)
    {
        return new StubHandler(type, datasetsWithData ?? Array.Empty<Dataset>());
    }

    private sealed class StubHandler : IDatasetAvailabilityHandler
    {
        public DownloadType SupportedDownloadType { get; }
        private readonly IEnumerable<Dataset> _datasetsWithData;

        public StubHandler(DownloadType type, IEnumerable<Dataset> datasetsWithData)
        {
            SupportedDownloadType = type;
            _datasetsWithData = datasetsWithData;
        }

        public Task<IEnumerable<Dataset>> GetAvailableDatasetsAsync(IEnumerable<string> pupilIds)
        {
            return Task.FromResult(_datasetsWithData);
        }
    }
}
