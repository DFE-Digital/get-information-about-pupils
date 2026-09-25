using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Access.Policies;

public interface IDatasetAccessEvaluator
{
    bool HasAccess(IAuthorisationContext context, Dataset dataset);
}
