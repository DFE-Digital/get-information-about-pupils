using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Handlers;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability;

public interface IDatasetAvailabilityHandlerFactory
{
    IDatasetAvailabilityHandler GetDatasetAvailabilityHandler(PupilDownloadType type);
}
