using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access;

/// <summary>
/// Defines a method to determine whether a dataset can be downloaded by a user within a given authorization context.
/// </summary>
/// <remarks>Implementations should evaluate the provided authorization context and dataset to enforce access
/// control policies. This interface is intended to be used by components that need to check download permissions before
/// allowing access to dataset content.</remarks>
public interface IDatasetAuthorisationService
{
    /// <summary>
    /// Determines whether the specified authorisation context permits downloading the given dataset.
    /// </summary>
    /// <param name="authorisationContext">The authorisation context representing the user's permissions and identity. Cannot be null.</param>
    /// <param name="dataset">The dataset for which download permission is being evaluated. Cannot be null.</param>
    /// <returns>true if the authorisation context allows downloading the dataset; otherwise, false.</returns>
    bool CanDownload(IAuthorisationContext authorisationContext, Dataset dataset);
}
