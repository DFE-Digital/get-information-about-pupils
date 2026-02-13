using DfE.GIAP.Web.Features.MyPupils.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Controllers;
internal static class ActionResultAssertionHelpers
{
    internal static void AssertRedirectToGetMyPupils(IActionResult controllerResponse, MyPupilsQueryRequestDto? query = null)
    {
        Assert.NotNull(controllerResponse);
        RedirectToActionResult result = Assert.IsType<RedirectToActionResult>(controllerResponse);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("GetMyPupils", result.ControllerName);

        query ??= new();
        Dictionary<string, object> routeValues = new()
        {
            { "PageNumber", query.PageNumber },
            { "SortField", query.SortField },
            { "SortDirection", query.SortDirection }
        };
        Assert.Equivalent(routeValues, result.RouteValues);
    }
}
