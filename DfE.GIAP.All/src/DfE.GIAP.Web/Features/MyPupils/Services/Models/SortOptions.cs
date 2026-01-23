namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;

public record SortOptions
{
    public SortOptions(string sortField, string sortDirection) // TODO valid sort fields from options
    {
        Field = string.IsNullOrWhiteSpace(sortField) ? string.Empty : sortField;

        Direction =
            string.IsNullOrWhiteSpace(sortDirection) ?
                SortDirection.NoneOrUnknown :
                    sortDirection.ToLowerInvariant() switch
                    {
                        "asc" => SortDirection.Ascending,
                        "desc" => SortDirection.Descending,
                        _ => SortDirection.NoneOrUnknown
                    };
    }

    public string Field { get; }
    public SortDirection Direction { get; }
}
