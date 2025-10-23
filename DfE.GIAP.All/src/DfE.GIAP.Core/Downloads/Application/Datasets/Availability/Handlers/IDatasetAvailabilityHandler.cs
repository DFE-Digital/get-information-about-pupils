using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;

public interface IDatasetAvailabilityHandler
{
    DownloadType SupportedDownloadType { get; }
    Task<IEnumerable<Dataset>> GetAvailableDatasetsAsync(IEnumerable<string> pupilIds);
}
