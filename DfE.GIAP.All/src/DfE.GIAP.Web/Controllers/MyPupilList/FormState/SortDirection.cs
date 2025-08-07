namespace DfE.GIAP.Web.Controllers.MyPupilList.FormState;
public enum SortDirection
{
    Ascending,
    Descending
}

public static class SortDirectionExtensions
{
    public static string ToFormState(this SortDirection direction)
        => direction switch
        {
            SortDirection.Ascending => "asc",
            SortDirection.Descending => "desc",
            _ => string.Empty
        };
}
