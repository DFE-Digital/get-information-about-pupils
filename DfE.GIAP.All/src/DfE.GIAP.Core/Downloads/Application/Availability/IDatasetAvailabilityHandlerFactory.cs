using DfE.GIAP.Core.Downloads.Application.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Availability;

public interface IDatasetAvailabilityHandlerFactory
{
    IDatasetAvailabilityHandler GetDatasetAvailabilityHandler(DownloadType type);
}
