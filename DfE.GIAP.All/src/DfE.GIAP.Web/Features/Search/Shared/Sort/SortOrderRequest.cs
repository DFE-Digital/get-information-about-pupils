namespace DfE.GIAP.Core.Search.Application.Models.Sort;
public record SortOrderRequest
{
    public SortOrderRequest(string searchKey, (string? Field, string? Direction) sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchKey);
        SearchKey = searchKey;

        ArgumentNullException.ThrowIfNull(sortOrder);
        SortOrder = sortOrder;
    }

    public string SearchKey { get; }
    public (string? field, string? direction) SortOrder { get; }
}
