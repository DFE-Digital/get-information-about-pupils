using Microsoft.AspNetCore.Http;
using Moq;

namespace DfE.GIAP.Web.Tests.TestDoubles.Http;
internal static class IHttpContextAccessorTestDoubles
{
    internal static Mock<IHttpContextAccessor> Default() => new();

    internal static Mock<IHttpContextAccessor> WithHttpContext(HttpContext? httpContext)
    {
        Mock<IHttpContextAccessor> httpContextAccessorMock = Default();
        httpContextAccessorMock.SetupGet(t => t.HttpContext).Returns(httpContext);
        return httpContextAccessorMock;
    }
}
