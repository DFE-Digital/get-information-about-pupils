using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Access.Rules;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Access.Rules.IndividualRules;

internal sealed class IsEstablishmentUserAccessRule : IDatasetAccessRule
{
    public bool HasAccess(IAuthorisationContext context) => context.IsEstablishment;
}
