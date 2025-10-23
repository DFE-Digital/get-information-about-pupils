using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access;

public class DatasetAccessService : IDatasetAuthorisationService
{
    public bool CanDownload(IAuthorisationContext context, Dataset dataset)
    {
        return DatasetAccessPolicy.IsAllowed(dataset, context);
    }
}
