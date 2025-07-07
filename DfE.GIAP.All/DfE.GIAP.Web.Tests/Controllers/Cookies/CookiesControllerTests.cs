using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Cookies
{
    [Trait("Category", "Cookies Controller Unit Tests")]
    public class CookiesControllerTests
    {
        private readonly Mock<ICookieManager> _mockCookieManager;

        public CookiesControllerTests()
        {
            _mockCookieManager = new Mock<ICookieManager>();
        }

        [Fact]
        public void Index_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var controller = GetCookiesController();

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CookiePreferencesViewModel>(viewResult.Model);
            Assert.True(model.CookieUse.IsCookieWebsiteUse);
            Assert.False(model.CookieUse.IsCookieComms);
        }

        [Fact]
        public void CookiePreferences_AppendsCookiesAndRedirects()
        {
            // Arrange
            var controller = GetCookiesController();
            var viewModel = new CookieUseViewModel
            {
                CookieWebsiteUse = Global.StatusTrue,
                CookieComms = Global.StatusFalse
            };

            var responseCookies = Mock.Get(controller.ControllerContext.HttpContext.Response.Cookies);

            // Act
            var result = controller.CookiePreferences(viewModel);

            // Assert
            responseCookies.Verify(
                c => c.Append(CookieKeys.GiapWebsiteUse, Global.StatusTrue, It.IsAny<CookieOptions>()),
                Times.Once);

            responseCookies.Verify(
                c => c.Append(CookieKeys.GiapComms, Global.StatusFalse, It.IsAny<CookieOptions>()),
                Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Landing", redirectResult.ControllerName);
        }

        [Fact]
        public void CookiePreferences_DoesNotAppendCookies_WhenValuesAreNullOrEmpty()
        {
            // Arrange
            var controller = GetCookiesController();
            var viewModel = new CookieUseViewModel
            {
                CookieWebsiteUse = null,
                CookieComms = string.Empty
            };

            var responseCookies = Mock.Get(controller.ControllerContext.HttpContext.Response.Cookies);

            // Act
            var result = controller.CookiePreferences(viewModel);

            // Assert
            responseCookies.Verify(
                c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()),
                Times.Never);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Landing", redirectResult.ControllerName);
        }

        private CookiesController GetCookiesController()
        {
            // Mock HttpContext
            var httpContext = new Mock<HttpContext>();

            // Mock Request Cookies
            var requestCookies = new Mock<IRequestCookieCollection>();
            requestCookies.Setup(c => c[CookieKeys.GiapWebsiteUse]).Returns(Global.StatusTrue);
            requestCookies.Setup(c => c[CookieKeys.GiapComms]).Returns(Global.StatusFalse);
            httpContext.Setup(c => c.Request.Cookies).Returns(requestCookies.Object);

            // Mock Response Cookies
            var responseCookies = new Mock<IResponseCookies>();
            responseCookies
                .Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()))
                .Verifiable();
            var response = new Mock<HttpResponse>();
            response.Setup(r => r.Cookies).Returns(responseCookies.Object);
            httpContext.Setup(c => c.Response).Returns(response.Object);

            // Set the mocked HttpContext
            var controller = new CookiesController(_mockCookieManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext.Object
                }
            };

            return controller;
        }
    }
}
