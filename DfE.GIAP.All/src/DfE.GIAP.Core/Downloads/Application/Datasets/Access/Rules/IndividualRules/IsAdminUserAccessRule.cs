using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;

/// <summary>
/// Represents an access rule that grants dataset download permissions to users identified as Admin users.
/// </summary>
/// <remarks>This rule grants access only to users with the "GAIPAdmin" role. It is typically used to restrict
/// certain dataset actions to administrative users within the authorization context.</remarks>
internal sealed class IsAdminUserAccessRule : IDatasetAccessRule
{
    public bool HasAccess(IAuthorisationContext context) => context.Role == "GIAPAdmin";
}
