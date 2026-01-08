using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Availability;

public interface IDatasetAvailabilityHandlerFactory
{
    IDatasetAvailabilityHandler GetDatasetAvailabilityHandler(DownloadType type);
}
