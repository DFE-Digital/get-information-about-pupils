using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;

/// <summary>
/// Represents a dataset access rule that always allows access, regardless of the authorization context.
/// </summary>
/// <remarks>This rule can be used in scenarios where unrestricted access is required, such as for public datasets
/// or testing purposes. It implements the IDatasetAccessRule interface and always grants permission when
/// evaluated.</remarks>
internal sealed class AlwaysAllowAccessRule : IDatasetAccessRule
{
    public bool HasAccess(IAuthorisationContext context) => true;
}
