using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.ViewModels.Search;
using static DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByName.FurtherEducationLearnerTextSearchResponseToViewModelMapper;

namespace DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByName;

/// <summary>
/// Maps a <see cref="LearnerSearchMappingContext"/> — which contains both the search response
/// and the existing view model — into a fully populated <see cref="LearnerTextSearchViewModel"/>.
/// This mapper bridges domain-layer search results with UI-facing representations.
/// </summary>
public sealed class FurtherEducationLearnerTextSearchResponseToViewModelMapper :
    IMapper<FurtherEducationLearnerTextSearchMappingContext, LearnerTextSearchViewModel>
{
    // Mapper for converting individual FurtherEducationLearner domain entities into UI-facing Learner view models.
    private readonly IMapper<FurtherEducationLearner, Learner> _furtherEducationLearnerToViewModelMapper;

    // Mapper for converting facet meta-data (SearchFacets) into structured filter data for the view model.
    private readonly IMapper<SearchFacets, List<FilterData>> _filtersResponseMapper;

    /// <summary>
    /// Constructs a new instance of <see cref="FurtherEducationLearnerTextSearchResponseToViewModelMapper"/>.
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
    public FurtherEducationLearnerTextSearchResponseToViewModelMapper(
        IMapper<FurtherEducationLearner, Learner> furtherEducationLearnerToViewModelMapper,
        IMapper<SearchFacets, List<FilterData>> filtersResponseMapper)
    {
        ArgumentNullException.ThrowIfNull(furtherEducationLearnerToViewModelMapper);
        _furtherEducationLearnerToViewModelMapper = furtherEducationLearnerToViewModelMapper;

        ArgumentNullException.ThrowIfNull(filtersResponseMapper);
        _filtersResponseMapper = filtersResponseMapper;
    }

    /// <summary>
    /// Maps the search response and existing model into a fully populated <see cref="FurtherEducationLearnerTextSearchMappingContext"/>.
    /// </summary>
    /// <param name="input">Encapsulated context containing the view model and search response.</param>
    /// <returns>A populated <see cref="FurtherEducationLearnerTextSearchMappingContext"/> ready for UI rendering.</returns>
    public LearnerTextSearchViewModel Map(FurtherEducationLearnerTextSearchMappingContext input)
    {
        // Map facet filters from the response into structured filter data for the view model.
        input.Model.Filters =
            _filtersResponseMapper.Map(input.Response.FacetedResults);

        // Map each learner from domain to view model using the injected learner mapper.
        List<Learner> learners =
            input.Response.LearnerSearchResults?.Learners
                .Select(_furtherEducationLearnerToViewModelMapper.Map)
                .ToList() ?? [];

        // Apply PageSize limit
        input.Model.Learners = learners.Take(input.Model.PageSize);

        // Populate meta-data fields for pagination and UI messaging.
        input.Model.Count = input.Response.LearnerSearchResults?.Count ?? 0;
        input.Model.Total = input.Response.TotalNumberOfResults + input.Model.Offset;

        return input.Model;
    }

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
        public FurtherEducationSearchByNameResponse Response { get; init; }

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
            FurtherEducationSearchByNameResponse response)
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
            FurtherEducationSearchByNameResponse response) =>
            new(model, response);
    }
}
