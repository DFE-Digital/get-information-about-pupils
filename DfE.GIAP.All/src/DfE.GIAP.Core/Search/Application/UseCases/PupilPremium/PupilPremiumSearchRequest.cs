using System.ComponentModel.DataAnnotations;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Sort;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
public sealed class PupilPremiumSearchRequest : IUseCaseRequest<PupilPremiumSearchResponse>
{
    public PupilPremiumSearchRequest(string searchKeywords, SortOrder sortOrder, int offset = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchKeywords);
        SearchKeywords = searchKeywords;

        ArgumentNullException.ThrowIfNull(sortOrder);
        SortOrder = sortOrder;

        Offset = offset;
    }

    public PupilPremiumSearchRequest(
        string searchKeywords,
        IList<FilterRequest> filterRequests,
        SortOrder sortOrder,
        int offset = 0) : this(searchKeywords, sortOrder, offset)
    {
        ArgumentNullException.ThrowIfNull(filterRequests);
        FilterRequests = filterRequests;
    }

    /// <summary>
    /// The keyword(s) used to search query data.
    /// </summary>
    public string SearchKeywords { get; }

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
