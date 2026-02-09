using System.ComponentModel.DataAnnotations;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;

namespace DfE.GIAP.Core.Search.Application.Services.SearchByName;
public record SearchLearnerByNameRequest
{
    public SearchLearnerByNameRequest(
        string searchKeywords,
        SearchCriteria searchCriteria,
        SortOrder sortOrder,
        IList<FilterRequest>? filters = null,
        int offset = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchKeywords);
        SearchKeywords = searchKeywords;

        ArgumentNullException.ThrowIfNull(searchCriteria);
        SearchCriteria = searchCriteria;

        ArgumentNullException.ThrowIfNull(sortOrder);
        SortOrder = sortOrder;

        FilterRequests = filters ?? [];

        Offset = offset;
    }

    public string SearchKeywords { get; }
    public SearchCriteria SearchCriteria { get; }
    public SortOrder SortOrder { get; }

    [Range(0, int.MaxValue, ErrorMessage = "Offset must be non-negative.")]
    public int Offset { get; }

    public IList<FilterRequest>? FilterRequests { get; }
}