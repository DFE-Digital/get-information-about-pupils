using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Request;
using DfE.GIAP.Core.Search.Application.UseCases.Response;

namespace DfE.GIAP.Core.Search.Application.UseCases;

/// <summary>
/// Use case responsible for executing a further education pupil search based on first name and/or surname.
/// It delegates the search operation to a domain-specific adapter and returns structured results.
/// </summary>
public sealed class SearchByKeyWordsUseCase :
    IUseCase<SearchByKeyWordsRequest, SearchByKeyWordsResponse>
{
    private readonly SearchCriteria _searchCriteria;
    private readonly ISearchServiceAdapter<Learners, SearchFacets> _searchServiceAdapter;

    /// <summary>
    /// Constructs a new instance of the use case with required dependencies.
    /// </summary>
    /// <param name="searchCriteria">Provides configuration for search fields and facets.</param>
    /// <param name="searchServiceAdapter">Adapter to interact with Azure Cognitive Search using domain models.</param>
    /// <exception cref="ArgumentNullException">Thrown if either dependency is null.</exception>
    public SearchByKeyWordsUseCase(
        SearchCriteria searchCriteria,
        ISearchServiceAdapter<Learners, SearchFacets> searchServiceAdapter)
    {
        _searchCriteria = searchCriteria ??
            throw new ArgumentNullException(nameof(searchCriteria));
        _searchServiceAdapter = searchServiceAdapter ??
            throw new ArgumentNullException(nameof(searchServiceAdapter));
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
    public async Task<SearchByKeyWordsResponse> HandleRequestAsync(
        SearchByKeyWordsRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SearchKeyword))
        {
            return new(SearchResponseStatus.InvalidRequest);
        }

        try
        {
            SearchResults<Learners, SearchFacets>? results =
                await _searchServiceAdapter.SearchAsync(
                    new SearchServiceAdapterRequest(
                        request.SearchKeyword,
                        _searchCriteria.SearchFields,
                        _searchCriteria.Facets,
                        request.SortOrder,
                        request.FilterRequests,
                        request.Offset));

            return results is null
                ? new(SearchResponseStatus.SearchServiceError)
                : new(SearchResponseStatus.Success)
                {
                    LearnerSearchResults = results.Results,
                    FacetedResults = results.FacetResults,
                    TotalNumberOfResults = (int)(results.TotalNumberOfRecords ?? 0)
                };
        }
        catch (Exception ex)
        {
            // TODO: Log the exception details for diagnostics.
            Console.WriteLine($"Search operation failed: {ex.Message}");
            // Handles unexpected failures such as adapter exceptions or infrastructure issues.
            return new(SearchResponseStatus.SearchServiceError);
        }
    }
}
