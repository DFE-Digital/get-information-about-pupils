using DfE.GIAP.Core.Downloads.Application.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class DatasetAvailabilityHandlerTestDouble
{
    public static IDatasetAvailabilityHandler Create(
        PupilDownloadType type,
        IEnumerable<Dataset>? datasetsWithData = null)
    {
        return new StubHandler(type, datasetsWithData ?? Array.Empty<Dataset>());
    }

    private sealed class StubHandler : IDatasetAvailabilityHandler
    {
        public PupilDownloadType SupportedDownloadType { get; }
        private readonly IEnumerable<Dataset> _datasetsWithData;

        public StubHandler(PupilDownloadType type, IEnumerable<Dataset> datasetsWithData)
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
