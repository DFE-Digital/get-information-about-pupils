using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Controllers.LearnerNumber.Mappers;

public sealed class LearnerNumericSearchResponseToViewModelMapper :
    IMapper<LearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>
{
    // Mapper for converting individual FurtherEducationLearner domain entities into UI-facing Learner view models.
    private readonly IMapper<Core.Search.Application.Models.Learner.Learner, Learner> _furtherEducationLearnerToViewModelMapper;

    /// <summary>
    /// Constructs a new instance of <see cref="LearnerNumericSearchResponseToViewModelMapper"/>.
    /// </summary>
    /// <param name="furtherEducationLearnerToViewModelMapper">
    /// Mapper used to convert individual learners from domain to view model.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either mapper dependency is null, ensuring safe construction.
    /// </exception>
    public LearnerNumericSearchResponseToViewModelMapper(
        IMapper<Core.Search.Application.Models.Learner.Learner, Learner> furtherEducationLearnerToViewModelMapper)
    {
        _furtherEducationLearnerToViewModelMapper = furtherEducationLearnerToViewModelMapper ??
            throw new ArgumentNullException(nameof(furtherEducationLearnerToViewModelMapper));
    }

    public LearnerNumberSearchViewModel Map(LearnerNumericSearchMappingContext input)
    {
        // Map each learner from domain to view model using the injected learner mapper.
        List<Learner> learners =
            input.Response.LearnerSearchResults?.LearnerCollection
                .Select(_furtherEducationLearnerToViewModelMapper.Map)
                .ToList() ?? [];

        // Apply result limit if the total exceeds the configured maximum.
        input.Model.Learners =
            input.Response.TotalNumberOfResults > input.Model.MaximumResults
                ? [.. learners.Take(input.Model.MaximumResults)]
                : learners;

        // Populate meta-data fields for pagination and UI messaging.
        input.Model.Total = input.Response.TotalNumberOfResults;

        return input.Model;
    }
}

/// <summary>
/// Encapsulates the inputs required to map a learner text search response into a view model.
/// This wrapper replaces tuple usage to improve readability, semantic clarity, and extensibility.
/// </summary>
public sealed class LearnerNumericSearchMappingContext
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
    public SearchResponse Response { get; init; }

    /// <summary>
    /// Constructs a new <see cref="LearnerTextSearchMappingContext"/> with required inputs.
    /// Performs null checks to ensure safe downstream mapping.
    /// </summary>
    /// <param name="model">The target view model to populate.</param>
    /// <param name="response">The search response to map from.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="model"/> or <paramref name="response"/> is null.
    /// </exception>
    public LearnerNumericSearchMappingContext(
        LearnerNumberSearchViewModel model,
        SearchResponse response)
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
    public static LearnerNumericSearchMappingContext Create(
        LearnerNumberSearchViewModel model,
        SearchResponse response) =>
        new(model, response);
}
