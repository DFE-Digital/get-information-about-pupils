using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.Extensions;

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
