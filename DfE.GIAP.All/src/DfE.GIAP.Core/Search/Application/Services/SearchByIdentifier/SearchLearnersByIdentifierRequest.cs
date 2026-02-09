using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;

namespace DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;
internal record SearchLearnersByIdentifierRequest
{
    internal SearchLearnersByIdentifierRequest(
        string[] identifiers,
        SearchCriteria searchCriteria,
        SortOrder sort,
        int offset = 0)
    {
        if (identifiers is null || identifiers.Length == 0)
        {
            throw new ArgumentException("Identifiers cannot be null or empty.");
        }
        Identifiers = identifiers.Distinct().ToList().AsReadOnly();

        ArgumentNullException.ThrowIfNull(searchCriteria);
        SearchCriteria = searchCriteria;

        ArgumentNullException.ThrowIfNull(sort);
        Sort = sort;

        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        Offset = offset;
    }

    public IReadOnlyList<string> Identifiers { get; }
    public SearchCriteria SearchCriteria { get; }
    public SortOrder Sort { get; }
    public int Offset { get; }
}
