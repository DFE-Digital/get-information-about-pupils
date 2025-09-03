using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using System.ComponentModel.DataAnnotations;

namespace DfE.GIAP.Core.Search.Application.UseCases.Request;

/// <summary>
/// Represents a search request for pupils by first name and/or surname,
/// optionally scoped with filters and offset for pagination.
/// </summary>
public sealed class SearchByKeyWordsRequest
    : IUseCaseRequest<SearchByKeyWordsResponse>
{
    /// <summary>
    /// Initializes a basic search request with keyword and optional offset.
    /// </summary>
    /// <param name="searchKeyword">The keyword used to query pupil names.</param>
    /// <param name="offset">Offset for pagination (defaults to 0).</param>
    /// <exception cref="ArgumentException">Thrown if searchKeyword is null or empty.</exception>
    public SearchByKeyWordsRequest(string searchKeyword, SortOrder sortOrder, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(searchKeyword))
        {
            throw new ArgumentException(
                "Search keyword must not be null or empty.", nameof(searchKeyword));
        }

        SearchKeyword = searchKeyword;
        SortOrder = sortOrder ?? throw new ArgumentNullException(nameof(sortOrder));
        Offset = offset;
    }

    /// <summary>
    /// Initializes a filtered search request.
    /// </summary>
    /// <param name="searchKeyword">The search keyword.</param>
    /// <param name="filterRequests">A list of filter criteria.</param>
    /// <param name="offset">Offset for pagination (defaults to 0).</param>
    public SearchByKeyWordsRequest(
        string searchKeyword,
        IList<FilterRequest> filterRequests,
        SortOrder sortOrder,
        int offset = 0) : this(searchKeyword, sortOrder, offset)
    {
        FilterRequests = filterRequests ??
            throw new ArgumentNullException(nameof(filterRequests));
    }

    /// <summary>
    /// The name keyword used to query pupil data.
    /// </summary>
    public string SearchKeyword { get; }

    /// <summary>
    /// The offset used for paging through search results.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Offset must be non-negative.")]
    public int Offset { get; }

    /// <summary>
    /// Optional filters used to narrow down the search results.
    /// </summary>
    public IList<FilterRequest>? FilterRequests { get; }

    /// <summary>
    /// Specifies the order in which search results should be sorted.
    /// </summary>
    public SortOrder SortOrder { get; }
}
