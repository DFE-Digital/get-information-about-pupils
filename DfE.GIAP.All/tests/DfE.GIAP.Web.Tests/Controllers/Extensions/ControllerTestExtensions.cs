using System.Security.Claims;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

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

    internal static TempDataDictionary StubTempData<T>(
        this T controller,
        Dictionary<string, object?>? tempDataDictionaryStub = null,
        HttpContext? httpContext = null) where T : Controller
    {
        Mock<ITempDataProvider> providerMock = new();

        providerMock
            .Setup((provider) => provider.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(tempDataDictionaryStub ?? []);

        TempDataDictionary tempData = new(
            httpContext ?? new DefaultHttpContext(),
            providerMock.Object);

        controller.TempData = tempData;
        return tempData;
    }

    internal static T AddModelStateError<T>(
        this T controller,
        Exception? exception = null,
        string? message = null) where T : ControllerBase
    {
        controller.ModelState.TryAddModelException(
            message ?? "test-error",
            exception ?? new Exception("test"));

        return controller;
    }
} 
