using DfE.GIAP.Core.Downloads.Application.Availability.Access.Policies;

namespace DfE.GIAP.Core.Downloads.Application.Availability.Access.Rules;

/// <summary>
/// Defines a rule that determines whether a dataset can be accessed based on the provided authorization context.
/// </summary>
/// <remarks>Implementations of this interface encapsulate access control logic for datasets. Use this
/// interface to enforce custom authorization policies when granting or denying access permissions.</remarks>
public interface IDatasetAccessRule
{
    /// <summary>
    /// Determines whether the specified authorization context permits access to the content.
    /// </summary>
    /// <param name="context">The authorization context to evaluate for download permissions. Cannot be null.</param>
    /// <returns>true if the context allows access; otherwise, false.</returns>
    bool HasAccess(IAuthorisationContext context);
}
