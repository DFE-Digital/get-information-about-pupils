using DfE.GIAP.Web.Constants;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers;

internal static class MyPupilsRedirectHelpers
{
    internal static RedirectToActionResult RedirectToGetMyPupils(MyPupilsQueryRequestDto request)
    {
        return new RedirectToActionResult(
            actionName: "Index",
            controllerName: Routes.MyPupilList.GetMyPupilsController,
            new
            {
                request.PageNumber,
                request.SortField,
                request.SortDirection
            });
    }
}
