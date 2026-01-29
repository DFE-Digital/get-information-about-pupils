using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// Provides a test double for <see cref="Pageable{T}"/> to simulate paginated Azure Search results.
/// Enables deterministic unit testing of search result consumers without relying on live Azure Search responses.
/// </summary>
internal static class PageableTestDouble
{
    public static Pageable<SearchResult<TDataTransferObject>> FromResults<TDataTransferObject>(
        List<SearchResult<TDataTransferObject>> results)
    {
        // Create a single page from the provided results, with no continuation token
        Page<SearchResult<TDataTransferObject>> page =
            Page<SearchResult<TDataTransferObject>>.FromValues(
                results,
                continuationToken: null,
                new Mock<Response>().Object); // Mocked Azure response metadata

        // Wrap the page in a pageable sequence to simulate Azure's paginated result structure
        return Pageable<SearchResult<TDataTransferObject>>
            .FromPages(new List<Page<SearchResult<TDataTransferObject>>>() { page });
    }
}
