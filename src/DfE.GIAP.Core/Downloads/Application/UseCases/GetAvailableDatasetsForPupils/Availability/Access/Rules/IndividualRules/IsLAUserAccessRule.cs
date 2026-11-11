using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Access.Rules;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Access.Rules.IndividualRules;

internal sealed class IsLAUserAccessRule : IDatasetAccessRule
{
    public bool HasAccess(IAuthorisationContext context) => context.IsLAUser;
}
