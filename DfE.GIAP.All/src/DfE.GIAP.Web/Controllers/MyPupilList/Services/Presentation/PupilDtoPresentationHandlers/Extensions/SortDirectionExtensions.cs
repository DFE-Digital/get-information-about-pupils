using DfE.GIAP.Web.Controllers.MyPupilList.Services.Presentation.PupilDtoPresentationHandlers.Options;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presentation.PupilDtoPresentationHandlers.Extensions;

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
