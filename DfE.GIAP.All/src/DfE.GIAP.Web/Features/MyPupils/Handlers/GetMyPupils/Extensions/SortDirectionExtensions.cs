using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Extensions;

public static class SortDirectionExtensions
{
    public static string ToFormSortDirection(this SortDirection sortDirection)
    {
        return sortDirection switch
        {
            SortDirection.Ascending => "asc",
            SortDirection.Descending => "desc",
            _ => string.Empty
        };
    }
}
