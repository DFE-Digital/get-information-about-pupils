using System.Security.Claims;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Tests.Controllers.Extensions;
internal static class ControllerTestExtensions
{
    internal static HttpContext StubHttpContext<T>(this T controller) where T : ControllerBase
    {
        // TODO may want control of this principal on the context?
        ClaimsPrincipal claimsPrincipal = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();

        DefaultHttpContext httpContext = new()
        {
            User = claimsPrincipal,
            Session = new TestSession()
        };

        ControllerContext controllerContext = new()
        {
            HttpContext = httpContext
        };

        controller.ControllerContext = controllerContext;

        return httpContext;
    }
}
