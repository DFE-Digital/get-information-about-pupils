using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.ViewModels.Search;
using static DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers.LearnerTextSearchResponseToViewModelMapper;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;

/// <summary>
/// Maps a <see cref="LearnerSearchMappingContext"/> — which contains both the search response
/// and the existing view model — into a fully populated <see cref="LearnerTextSearchViewModel"/>.
/// This mapper bridges domain-layer search results with UI-facing representations.
/// </summary>
public sealed class LearnerTextSearchResponseToViewModelMapper :
    IMapper<LearnerTextSearchMappingContext, LearnerTextSearchViewModel>
{
    // Mapper for converting individual FurtherEducationLearner domain entities into UI-facing Learner view models.
    private readonly IMapper<Core.Search.Application.Models.Learner.Learner, Learner> _furtherEducationLearnerToViewModelMapper;

    // Mapper for converting facet meta-data (SearchFacets) into structured filter data for the view model.
    private readonly IMapper<SearchFacets, List<FilterData>> _filtersResponseMapper;

    /// <summary>
    /// Constructs a new instance of <see cref="LearnerTextSearchResponseToViewModelMapper"/>.
    /// </summary>
    /// <param name="furtherEducationLearnerToViewModelMapper">
    /// Mapper used to convert individual learners from domain to view model.
    /// </param>
    /// <param name="filtersResponseMapper">
    /// Mapper used to convert facet meta-data into filter data for the UI.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either mapper dependency is null, ensuring safe construction.
    /// </exception>
    public LearnerTextSearchResponseToViewModelMapper(
        IMapper<Core.Search.Application.Models.Learner.Learner, Learner> furtherEducationLearnerToViewModelMapper,
        IMapper<SearchFacets, List<FilterData>> filtersResponseMapper)
    {
        _furtherEducationLearnerToViewModelMapper = furtherEducationLearnerToViewModelMapper ??
            throw new ArgumentNullException(nameof(furtherEducationLearnerToViewModelMapper));
        _filtersResponseMapper = filtersResponseMapper ??
            throw new ArgumentNullException(nameof(filtersResponseMapper));
    }

    /// <summary>
    /// Maps the search response and existing model into a fully populated <see cref="LearnerTextSearchMappingContext"/>.
    /// </summary>
    /// <param name="input">Encapsulated context containing the view model and search response.</param>
    /// <returns>A populated <see cref="LearnerTextSearchMappingContext"/> ready for UI rendering.</returns>
    public LearnerTextSearchViewModel Map(LearnerTextSearchMappingContext input)
    {
        // Map facet filters from the response into structured filter data for the view model.
        input.Model.Filters =
            _filtersResponseMapper.Map(input.Response.FacetedResults);

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
        input.Model.Count = input.Response.LearnerSearchResults?.Count ?? 0;
        input.Model.Total = input.Response.TotalNumberOfResults;
        input.Model.ShowOverLimitMessage = input.Model.Total > 100_000;

        return input.Model;
    }

    /// <summary>
    /// Encapsulates the inputs required to map a learner text search response into a view model.
    /// This wrapper replaces tuple usage to improve readability, semantic clarity, and extensibility.
    /// </summary>
    public sealed class LearnerTextSearchMappingContext
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
        public LearnerTextSearchMappingContext(
            LearnerTextSearchViewModel model,
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
        public static LearnerTextSearchMappingContext Create(
            LearnerTextSearchViewModel model,
            SearchResponse response) =>
            new(model, response);
    }
}
