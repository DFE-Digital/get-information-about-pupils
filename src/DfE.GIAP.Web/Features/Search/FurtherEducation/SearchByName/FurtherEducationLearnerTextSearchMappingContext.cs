using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByName;

/// <summary>
/// Encapsulates the inputs required to map a learner text search response into a view model.
/// This wrapper replaces tuple usage to improve readability, semantic clarity, and extensibility.
/// </summary>
public sealed class FurtherEducationLearnerTextSearchMappingContext
{
    /// <summary>
    /// The existing view model instance to be populated with search results.
    /// Typically contains user-entered filters, pagination settings, and result limits.
    /// </summary>
    public LearnerTextSearchViewModel Model { get; init; }

    /// <summary>
    /// The search response returned from the application layer.
    /// Contains learner results, facet filters, and meta-data such as total counts.
    /// </summary>
    public SearchResponse<FurtherEducationLearners> Response { get; init; }

    /// <summary>
    /// Constructs a new <see cref="FurtherEducationLearnerTextSearchMappingContext"/> with required inputs.
    /// Performs null checks to ensure safe downstream mapping.
    /// </summary>
    /// <param name="model">The target view model to populate.</param>
    /// <param name="response">The search response to map from.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="model"/> or <paramref name="response"/> is null.
    /// </exception>
    public FurtherEducationLearnerTextSearchMappingContext(
        LearnerTextSearchViewModel model,
        SearchResponse<FurtherEducationLearners> response)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Response = response ?? throw new ArgumentNullException(nameof(response));
    }

    /// <summary>
    /// Factory method for creating a new <see cref="FurtherEducationLearnerTextSearchMappingContext"/>.
    /// Improves readability and discoverability when constructing context objects.
    /// </summary>
    /// <param name="model">The target view model to populate.</param>
    /// <param name="response">The search response to map from.</param>
    /// <returns>A new instance of <see cref="FurtherEducationLearnerTextSearchMappingContext"/>.</returns>
    public static FurtherEducationLearnerTextSearchMappingContext Create(
        LearnerTextSearchViewModel model,
        SearchResponse<FurtherEducationLearners> response) =>
        new(model, response);
}
