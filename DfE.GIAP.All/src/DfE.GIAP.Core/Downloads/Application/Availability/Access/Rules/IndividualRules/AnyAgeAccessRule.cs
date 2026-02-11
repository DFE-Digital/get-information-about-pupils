using DfE.GIAP.Core.Downloads.Application.Availability.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Availability.Access.Rules;

namespace DfE.GIAP.Core.Downloads.Application.Availability.Access.Rules.IndividualRules;

internal class AnyAgeAccessRule : IDatasetAccessRule
{
    public bool HasAccess(IAuthorisationContext context) => context.AnyAgeUser;
}
