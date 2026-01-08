using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

public interface IDatasetAccessEvaluator
{
    bool HasAccess(IAuthorisationContext context, Dataset dataset);
}
