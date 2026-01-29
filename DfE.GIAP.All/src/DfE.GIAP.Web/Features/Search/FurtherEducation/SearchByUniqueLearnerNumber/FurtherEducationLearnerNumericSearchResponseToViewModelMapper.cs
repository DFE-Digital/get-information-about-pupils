using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByUniqueLearnerNumber;

public sealed class FurtherEducationLearnerNumericSearchResponseToViewModelMapper :
    IMapper<FurtherEducationLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>
{
    // Mapper for converting individual FurtherEducationLearner domain entities into UI-facing Learner view models.
    private readonly IMapper<FurtherEducationLearner, Learner> _furtherEducationLearnerToViewModelMapper;

    /// <summary>
    /// Constructs a new instance of <see cref="FurtherEducationLearnerNumericSearchResponseToViewModelMapper"/>.
    /// </summary>
    /// <param name="furtherEducationLearnerToViewModelMapper">
    /// Mapper used to convert individual learners from domain to view model.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either mapper dependency is null, ensuring safe construction.
    /// </exception>
    public FurtherEducationLearnerNumericSearchResponseToViewModelMapper(
        IMapper<FurtherEducationLearner, Learner> furtherEducationLearnerToViewModelMapper)
    {
        _furtherEducationLearnerToViewModelMapper = furtherEducationLearnerToViewModelMapper ??
            throw new ArgumentNullException(nameof(furtherEducationLearnerToViewModelMapper));
    }

    public LearnerNumberSearchViewModel Map(FurtherEducationLearnerNumericSearchMappingContext input)
    {
        // Map each learner from domain to view model using the injected learner mapper.
        List<Learner> learners =
            input.Response.LearnerSearchResults?.LearnerCollection
                .Select(_furtherEducationLearnerToViewModelMapper.Map)
                .ToList() ?? [];

        input.Model.Learners = learners;

        // Populate meta-data fields for pagination and UI messaging.
        input.Model.Total = input.Response.TotalNumberOfResults;

        return input.Model;
    }
}

/// <summary>
/// Encapsulates the inputs required to map a learner text search response into a view model.
/// This wrapper replaces tuple usage to improve readability, semantic clarity, and extensibility.
/// </summary>
public sealed class FurtherEducationLearnerNumericSearchMappingContext
{
    /// <summary>
    /// The existing view model instance to be populated with search results.
    /// Typically contains user-entered filters, pagination settings, and result limits.
    /// </summary>
    public LearnerNumberSearchViewModel Model { get; init; }

    /// <summary>
    /// The search response returned from the application layer.
    /// Contains learner results, facet filters, and meta-data such as total counts.
    /// </summary>
    public FurtherEducationSearchResponse Response { get; init; }

    /// <summary>
    /// Constructs a new <see cref="LearnerTextSearchMappingContext"/> with required inputs.
    /// Performs null checks to ensure safe downstream mapping.
    /// </summary>
    /// <param name="model">The target view model to populate.</param>
    /// <param name="response">The search response to map from.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="model"/> or <paramref name="response"/> is null.
    /// </exception>
    public FurtherEducationLearnerNumericSearchMappingContext(
        LearnerNumberSearchViewModel model,
        FurtherEducationSearchResponse response)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Response = response ?? throw new ArgumentNullException(nameof(response));
    }

    /// <summary>
    /// Factory method for creating a new <see cref="LearnerTextSearchMappingContext"/>.
    /// Improves readability and discoverability when constructing context objects.
    /// </summary>
    /// <param name="model">The target view model to populate.</param>
    /// <param name="response">The search response to map from.</param>
    /// <returns>A new instance of <see cref="LearnerTextSearchMappingContext"/>.</returns>
    public static FurtherEducationLearnerNumericSearchMappingContext Create(
        LearnerNumberSearchViewModel model,
        FurtherEducationSearchResponse response) =>
        new(model, response);
}
