using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DfE.GIAP.Web.Tests.Shared.HttpContext;
internal static class HttpContextTestDoubles
{
    internal static Microsoft.AspNetCore.Http.HttpContext Stub() => new DefaultHttpContext();

    internal static Microsoft.AspNetCore.Http.HttpContext WithSession(ISession? session) => new DefaultHttpContext()
    {
        Session = session!
    };

    internal static Microsoft.AspNetCore.Http.HttpContext WithUser(ClaimsPrincipal user) => new DefaultHttpContext()
    {
        User = user
    };
}
