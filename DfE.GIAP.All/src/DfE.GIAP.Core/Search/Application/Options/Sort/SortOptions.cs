namespace DfE.GIAP.Web.Features.Search.Options.Sort;

public sealed class SortOptions
{
    public List<string>? Fields { get; set; }
    public string? DefaultField { get; set; }
    public string? DefaultDirection { get; set; }

    public (string field, string direction) GetDefaultSort()
    {
        return
            (field: string.IsNullOrWhiteSpace(DefaultField) ? "search.score()" : DefaultField,
            direction: string.IsNullOrWhiteSpace(DefaultDirection) ? "desc" : DefaultDirection);
    }
}
