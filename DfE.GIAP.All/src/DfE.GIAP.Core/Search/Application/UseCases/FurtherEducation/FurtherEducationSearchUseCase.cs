using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation;

/// <summary>
/// Use case responsible for executing a further education pupil search based on first name and/or surname.
/// It delegates the search operation to a domain-specific adapter and returns structured results.
/// </summary>
public sealed class FurtherEducationSearchUseCase :
    IUseCase<FurtherEducationSearchRequest, FurtherEducationSearchResponse>
{
    private readonly ISearchServiceAdapter<FurtherEducationLearners, SearchFacets> _searchServiceAdapter;

    /// <summary>
    /// Constructs a new instance of the use case with required dependencies.
    /// </summary>
    /// <param name="searchCriteria">Provides configuration for search fields and facets.</param>
    /// <param name="searchServiceAdapter">Adapter to interact with Azure Cognitive Search using domain models.</param>
    /// <exception cref="ArgumentNullException">Thrown if either dependency is null.</exception>
    public FurtherEducationSearchUseCase(
        ISearchServiceAdapter<FurtherEducationLearners, SearchFacets> searchServiceAdapter)
    {
        ArgumentNullException.ThrowIfNull(searchServiceAdapter);
        _searchServiceAdapter = searchServiceAdapter;
    }

    /// <summary>
    /// Executes the search operation with the provided request.
    /// Validates the keyword, applies configured search criteria, and returns results or error status.
    /// </summary>
    /// <param name="request">The search request containing a keyword, optional filters, and an offset.</param>
    /// <returns>
    /// A structured response containing matched pupil records, faceted data, total results count,
    /// or an appropriate error status.
    /// </returns>
    public async Task<FurtherEducationSearchResponse> HandleRequestAsync(
        FurtherEducationSearchRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SearchKeywords))
        {
            return new(SearchResponseStatus.InvalidRequest);
        }

        try
        {
            SearchResults<FurtherEducationLearners, SearchFacets>? searchResults =
                await _searchServiceAdapter.SearchAsync(
                    new SearchServiceAdapterRequest(
                        searchIndexKey: "further-education",
                        request.SearchKeywords,
                        request.SearchCriteria.SearchFields,
                        request.SortOrder,
                        request.SearchCriteria.Facets,
                        request.FilterRequests,
                        request.Offset));

            return searchResults?.Results?.Count > 0
                ? new(SearchResponseStatus.Success, searchResults.Results.Count)
                {
                    LearnerSearchResults = searchResults.Results,
                    FacetedResults = searchResults.FacetResults
                }
                : new(SearchResponseStatus.NoResultsFound);

        }
        catch (Exception)
        {
            // Handles unexpected failures such as adapter exceptions or infrastructure issues.
            return new(SearchResponseStatus.SearchServiceError);
        }
    }
}
