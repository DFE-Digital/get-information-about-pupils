namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;

/// <summary>
/// Represents an access rule that grants dataset download permissions to users identified as Department for Education
/// (DfE) users.
/// </summary>
internal sealed class IsDfEUserAccessRule : IDatasetAccessRule
{
    public bool CanDownload(IAuthorisationContext context) => context.IsDfeUser;
}
