using System.ComponentModel.DataAnnotations;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Sort;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase;
public record NationalPupilDatabaseSearchRequest : IUseCaseRequest<NationalPupilDatabaseSearchResponse>
{
    public NationalPupilDatabaseSearchRequest(string searchKeywords, SortOrder sortOrder, int offset = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchKeywords);
        SearchKeywords = searchKeywords;

        ArgumentNullException.ThrowIfNull(sortOrder);
        SortOrder = sortOrder;
        Offset = offset;
    }

    public NationalPupilDatabaseSearchRequest(
        string searchKeywords,
        IList<FilterRequest> filterRequests,
        SortOrder sortOrder,
        int offset = 0) : this(searchKeywords, sortOrder, offset)
    {
        ArgumentNullException.ThrowIfNull(filterRequests);
        FilterRequests = filterRequests;
    }

    public string SearchKeywords { get; }

    [Range(0, int.MaxValue, ErrorMessage = "Offset must be non-negative.")]
    public int Offset { get; }

    public IList<FilterRequest>? FilterRequests { get; }

    public SortOrder SortOrder { get; }
}
