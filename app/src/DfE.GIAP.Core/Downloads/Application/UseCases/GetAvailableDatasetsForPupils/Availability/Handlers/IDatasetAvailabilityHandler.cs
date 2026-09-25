using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Handlers;

public interface IDatasetAvailabilityHandler
{
    PupilDownloadType SupportedDownloadType { get; }
    Task<IEnumerable<Dataset>> GetAvailableDatasetsAsync(IEnumerable<string> pupilIds);
}
