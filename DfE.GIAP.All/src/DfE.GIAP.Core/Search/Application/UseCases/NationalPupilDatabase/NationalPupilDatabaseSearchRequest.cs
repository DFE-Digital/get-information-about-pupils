using System.ComponentModel.DataAnnotations;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase;
public record NationalPupilDatabaseSearchRequest : IUseCaseRequest<NationalPupilDatabaseSearchResponse>
{
    public NationalPupilDatabaseSearchRequest(string searchKeywords, SearchCriteria searchCriteria, SortOrder sortOrder, int offset = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchKeywords);
        SearchKeywords = searchKeywords;

        ArgumentNullException.ThrowIfNull(searchCriteria);
        SearchCriteria = searchCriteria;
        
        ArgumentNullException.ThrowIfNull(sortOrder);
        SortOrder = sortOrder;
        Offset = offset;
    }

    public NationalPupilDatabaseSearchRequest(
        string searchKeywords,
        IList<FilterRequest> filterRequests,
        SearchCriteria searchCriteria,
        SortOrder sortOrder,
        int offset = 0) : this(searchKeywords, searchCriteria, sortOrder, offset)
    {
        ArgumentNullException.ThrowIfNull(filterRequests);
        FilterRequests = filterRequests;
    }

    public string SearchKeywords { get; }
    public SearchCriteria SearchCriteria { get; }

    [Range(0, int.MaxValue, ErrorMessage = "Offset must be non-negative.")]
    public int Offset { get; }

    public IList<FilterRequest>? FilterRequests { get; }

    public SortOrder SortOrder { get; }
}
