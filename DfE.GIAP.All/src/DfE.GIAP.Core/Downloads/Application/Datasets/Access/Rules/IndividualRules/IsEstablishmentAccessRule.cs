using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;

internal sealed class IsEstablishmentAccessRule : IDatasetAccessRule
{
    public bool HasAccess(IAuthorisationContext context) => context.IsEstablishment;
}
