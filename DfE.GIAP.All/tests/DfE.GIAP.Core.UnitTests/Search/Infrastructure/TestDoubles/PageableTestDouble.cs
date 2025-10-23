using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// Provides a test double for <see cref="Pageable{T}"/> to simulate paginated Azure Search results.
/// Enables deterministic unit testing of search result consumers without relying on live Azure Search responses.
/// </summary>
internal static class PageableTestDouble
{
    /// <summary>
    /// Constructs a stubbed <see cref="Pageable{SearchResult{LearnerDataTransferObject}}"/> from a list of search results.
    /// Wraps the results in a single page with no continuation token, simulating a complete response.
    /// </summary>
    /// <param name="results">List of learner search results to include in the mock page.</param>
    /// <returns>A pageable object containing a single page of results for test scenarios.</returns>
    public static Pageable<SearchResult<LearnerDataTransferObject>> FromResults(
        List<SearchResult<LearnerDataTransferObject>> results)
    {
        // Create a single page from the provided results, with no continuation token
        Page<SearchResult<LearnerDataTransferObject>> page =
            Page<SearchResult<LearnerDataTransferObject>>.FromValues(
                results,
                continuationToken: null,
                new Mock<Response>().Object); // Mocked Azure response metadata

        // Wrap the page in a pageable sequence to simulate Azure's paginated result structure
        return Pageable<SearchResult<LearnerDataTransferObject>>
            .FromPages(new List<Page<SearchResult<LearnerDataTransferObject>>>() { page });
    }
}
