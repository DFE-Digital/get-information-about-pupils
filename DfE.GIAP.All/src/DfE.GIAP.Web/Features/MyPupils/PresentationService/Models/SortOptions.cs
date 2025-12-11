namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;

public record SortOptions
{
    public SortOptions(string sortField, string sortDirection) // TODO valid sort fields from options
    {
        Field = string.IsNullOrWhiteSpace(sortField) ? string.Empty : sortField;

        Direction =
            sortDirection is null ? SortDirection.Descending :
                sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    ? SortDirection.Ascending : SortDirection.Descending;
    }

    public string Field { get; }
    public SortDirection Direction { get; }
}
