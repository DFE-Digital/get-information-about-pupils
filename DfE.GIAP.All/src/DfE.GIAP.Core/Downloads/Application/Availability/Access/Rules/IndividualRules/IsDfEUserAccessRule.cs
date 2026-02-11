using DfE.GIAP.Core.Downloads.Application.Availability.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Availability.Access.Rules;

namespace DfE.GIAP.Core.Downloads.Application.Availability.Access.Rules.IndividualRules;

/// <summary>
/// Represents an access rule that grants dataset download permissions to users identified as Department for Education
/// (DfE) users.
/// </summary>
internal sealed class IsDfEUserAccessRule : IDatasetAccessRule
{
    public bool HasAccess(IAuthorisationContext context) => context.IsDfeUser;
}
