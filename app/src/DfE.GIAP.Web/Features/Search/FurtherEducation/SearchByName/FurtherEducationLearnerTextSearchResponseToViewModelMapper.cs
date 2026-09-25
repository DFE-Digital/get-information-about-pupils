using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;
using DfE.GIAP.Web.ViewModels.Search;

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
}
