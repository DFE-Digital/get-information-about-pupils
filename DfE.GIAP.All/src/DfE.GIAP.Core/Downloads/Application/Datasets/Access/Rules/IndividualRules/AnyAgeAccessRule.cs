using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;

internal class AnyAgeAccessRule : IDatasetAccessRule
{
    public bool HasAccess(IAuthorisationContext context) => context.AnyAgeUser;
}
