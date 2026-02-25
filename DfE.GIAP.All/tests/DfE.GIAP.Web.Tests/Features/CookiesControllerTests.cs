using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features;
using DfE.GIAP.Web.Providers.Cookie;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        CookieUseViewModel viewModel = new()
        {
            CookieWebsiteUse = Global.StatusTrue,
            CookieComms = Global.StatusFalse
        };

        // Act
        IActionResult result = controller.CookiePreferences(viewModel);

        // Assert – verify calls to ICookieProvider
        _mockCookieProvider.Verify(
            x => x.Set(CookieKeys.GiapWebsiteUse, Global.StatusTrue),
            Times.Once);

        _mockCookieProvider.Verify(
            x => x.Set(CookieKeys.GiapComms, Global.StatusFalse),
            Times.Once);

        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public void CookiePreferences_DoesNotAppendCookies_WhenValuesAreNullOrEmpty()
    {
        // Arrange
        CookiesController controller = GetCookiesController();

        CookieUseViewModel viewModel = new()
        {
            CookieWebsiteUse = null,
            CookieComms = string.Empty
        };

        // Act
        IActionResult result = controller.CookiePreferences(viewModel);

        // Assert – verify Set was still called, but with null/empty
        _mockCookieProvider.Verify(
            x => x.Set(CookieKeys.GiapWebsiteUse, null),
            Times.Once);

        _mockCookieProvider.Verify(
            x => x.Set(CookieKeys.GiapComms, string.Empty),
            Times.Once);

        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    private CookiesController GetCookiesController()
    {
        // Mock HttpContext
        Mock<HttpContext> httpContext = new();

        // Mock Request Cookies
        Mock<IRequestCookieCollection> requestCookies = new();
        requestCookies.Setup(c => c[CookieKeys.GiapWebsiteUse]).Returns(Global.StatusTrue);
        requestCookies.Setup(c => c[CookieKeys.GiapComms]).Returns(Global.StatusFalse);
        httpContext.Setup(c => c.Request.Cookies).Returns(requestCookies.Object);

        // Mock Response Cookies (not used by controller, but required for HttpContext)
        Mock<IResponseCookies> responseCookies = new();
        Mock<HttpResponse> response = new();
        response.Setup(r => r.Cookies).Returns(responseCookies.Object);
        httpContext.Setup(c => c.Response).Returns(response.Object);

        return new CookiesController(_mockCookieProvider.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext.Object
            }
        };
    }
}
