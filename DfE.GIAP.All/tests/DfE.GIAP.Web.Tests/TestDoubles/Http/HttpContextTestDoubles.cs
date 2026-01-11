using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DfE.GIAP.Web.Tests.TestDoubles.Http;
internal static class HttpContextTestDoubles
{
    internal static HttpContext Stub() => new DefaultHttpContext();

    internal static HttpContext WithSession(ISession? session) => new DefaultHttpContext()
    {
        Session = session!
    };

    internal static HttpContext WithUser(ClaimsPrincipal user) => new DefaultHttpContext()
    {
        User = user
    };
}
