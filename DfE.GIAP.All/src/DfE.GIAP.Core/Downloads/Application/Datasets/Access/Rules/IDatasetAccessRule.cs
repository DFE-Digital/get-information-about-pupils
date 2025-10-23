namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;

/// <summary>
/// Defines a rule that determines whether a dataset can be downloaded based on the provided authorization context.
/// </summary>
/// <remarks>Implementations of this interface encapsulate access control logic for dataset downloads. Use this
/// interface to enforce custom authorization policies when granting or denying download permissions.</remarks>
public interface IDatasetAccessRule
{
    /// <summary>
    /// Determines whether the specified authorization context permits downloading content.
    /// </summary>
    /// <param name="context">The authorization context to evaluate for download permissions. Cannot be null.</param>
    /// <returns>true if the context allows downloading; otherwise, false.</returns>
    bool CanDownload(IAuthorisationContext context);
}
