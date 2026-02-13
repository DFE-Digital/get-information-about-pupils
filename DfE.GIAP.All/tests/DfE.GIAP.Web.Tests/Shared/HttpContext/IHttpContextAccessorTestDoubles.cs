using Microsoft.AspNetCore.Http;

namespace DfE.GIAP.Web.Tests.Shared.HttpContext;
internal static class IHttpContextAccessorTestDoubles
{
    internal static Mock<IHttpContextAccessor> Default() => new();

    internal static Mock<IHttpContextAccessor> WithHttpContext(Microsoft.AspNetCore.Http.HttpContext? httpContext)
    {
        Mock<IHttpContextAccessor> httpContextAccessorMock = Default();
        httpContextAccessorMock.SetupGet(t => t.HttpContext).Returns(httpContext);
        return httpContextAccessorMock;
    }
}
