using Microsoft.AspNetCore.Http;

namespace DfE.GIAP.Web.Tests.TestDoubles.Http;
internal static class HttpContextTestDoubles
{

    internal static HttpContext Default() => new DefaultHttpContext();

    internal static HttpContext WithSession(ISession? session) => new DefaultHttpContext()
    {
        Session = session!
    };
}
