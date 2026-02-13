using DfE.GIAP.Common.Constants;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features;
using DfE.GIAP.Web.Providers.Cookie;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features;

[Trait("Category", "Cookies Controller Unit Tests")]
public class CookiesControllerTests
{
    private readonly Mock<ICookieProvider> _mockCookieProvider;

    public CookiesControllerTests()
    {
        _mockCookieProvider = new Mock<ICookieProvider>();
    }

    [Fact]
    public void Index_ReturnsViewWithCorrectModel()
    {
        // Arrange
        CookiesController controller = GetCookiesController();

        // Act
        IActionResult result = controller.Index();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        CookiePreferencesViewModel model = Assert.IsType<CookiePreferencesViewModel>(viewResult.Model);
        Assert.True(model.CookieUse.IsCookieWebsiteUse);
        Assert.False(model.CookieUse.IsCookieComms);
    }

    [Fact]
    public void CookiePreferences_AppendsCookiesAndRedirects()
    {
        // Arrange
        CookiesController controller = GetCookiesController();
        CookieUseViewModel viewModel = new CookieUseViewModel
        {
            CookieWebsiteUse = Global.StatusTrue,
            CookieComms = Global.StatusFalse
        };

        Mock<IResponseCookies> responseCookies = Mock.Get(controller.ControllerContext.HttpContext.Response.Cookies);

        // Act
        IActionResult result = controller.CookiePreferences(viewModel);

        // Assert
        responseCookies.Verify(
            c => c.Append(CookieKeys.GiapWebsiteUse, Global.StatusTrue, It.IsAny<CookieOptions>()),
            Times.Once);

        responseCookies.Verify(
            c => c.Append(CookieKeys.GiapComms, Global.StatusFalse, It.IsAny<CookieOptions>()),
            Times.Once);

        RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
    }

    [Fact]
    public void CookiePreferences_DoesNotAppendCookies_WhenValuesAreNullOrEmpty()
    {
        // Arrange
        CookiesController controller = GetCookiesController();
        CookieUseViewModel viewModel = new CookieUseViewModel
        {
            CookieWebsiteUse = null,
            CookieComms = string.Empty
        };

        Mock<IResponseCookies> responseCookies = Mock.Get(controller.ControllerContext.HttpContext.Response.Cookies);

        // Act
        IActionResult result = controller.CookiePreferences(viewModel);

        // Assert
        responseCookies.Verify(
            c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()),
            Times.Never);

        RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
    }

    private CookiesController GetCookiesController()
    {
        // Mock HttpContext
        Mock<HttpContext> httpContext = new Mock<HttpContext>();

        // Mock Request Cookies
        Mock<IRequestCookieCollection> requestCookies = new Mock<IRequestCookieCollection>();
        requestCookies.Setup(c => c[CookieKeys.GiapWebsiteUse]).Returns(Global.StatusTrue);
        requestCookies.Setup(c => c[CookieKeys.GiapComms]).Returns(Global.StatusFalse);
        httpContext.Setup(c => c.Request.Cookies).Returns(requestCookies.Object);

        // Mock Response Cookies
        Mock<IResponseCookies> responseCookies = new Mock<IResponseCookies>();
        responseCookies
            .Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()))
            .Verifiable();
        Mock<HttpResponse> response = new Mock<HttpResponse>();
        response.Setup(r => r.Cookies).Returns(responseCookies.Object);
        httpContext.Setup(c => c.Response).Returns(response.Object);

        // Set the mocked HttpContext
        CookiesController controller = new CookiesController(_mockCookieProvider.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext.Object
            }
        };

        return controller;
    }
}
