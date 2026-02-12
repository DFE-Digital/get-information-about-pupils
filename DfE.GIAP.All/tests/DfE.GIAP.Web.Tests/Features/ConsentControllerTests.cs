using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Config;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features;
using DfE.GIAP.Web.Providers.Cookie;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features;

public sealed class ConsentControllerTests
{
    [Fact]
    public void Constructor_Throws_When_ConstructedWithNullSessionProvider()
    {
        // Arrange
        IOptions<AzureAppSettings> options = OptionsTestDoubles.Default<AzureAppSettings>();
        Mock<ICookieProvider> mockCookieProvider = new();

        // Act
        Func<ConsentController> construct = () => new ConsentController(
            null!,
            options,
            mockCookieProvider.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_ConstructedWithNullOptions()
    {
        // Arrange
        Mock<ISessionProvider> mockSessionProvider = new();
        Mock<ICookieProvider> mockCookieProvider = new();

        // Act
        Func<ConsentController> construct = () => new ConsentController(
            mockSessionProvider.Object,
            null!,
            mockCookieProvider.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_ConstructedWithNullOptionsValue()
    {
        // Arrange
        Mock<ISessionProvider> mockSessionProvider = new();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.MockNullOptions<AzureAppSettings>();
        Mock<ICookieProvider> mockCookieProvider = new();

        // Act
        Func<ConsentController> construct = () => new ConsentController(
            mockSessionProvider.Object,
            options,
            mockCookieProvider.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_ConstructedWithNullCookieManager()
    {
        // Arrange
        Mock<ISessionProvider> mockSessionProvider = new();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.Default<AzureAppSettings>();

        // Act
        Func<ConsentController> construct = () => new ConsentController(
            mockSessionProvider.Object,
            options,
            null!);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void GetIndex_Returns_ConsentView_And_SetsCookie_When_SessionIdStorageEnabled()
    {
        // Arrange
        Mock<ISessionProvider> mockSessionProvider = new();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.MockAs<AzureAppSettings>((t) => t.IsSessionIdStoredInCookie = true);
        Mock<ICookieProvider> mockCookieProvider = new();

        ConsentController controller = new(
            mockSessionProvider.Object,
            options,
            mockCookieProvider.Object);

        HttpContext context = controller.StubHttpContext();

        // Act
        IActionResult result = controller.Index();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        ConsentViewModel? viewModel = Assert.IsType<ConsentViewModel>(viewResult.Model);
        Assert.NotNull(viewModel);

        mockCookieProvider.Verify(
            (t) => t.Set(
                It.IsAny<string>(),
                context.User.GetSessionId(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<CookieOptions>()), Times.Once());
    }

    [Fact]
    public void GetIndex_Returns_ConsentView_And_DoesNotSetCookie_When_SessionIdStorageDisabled()
    {
        // Arrange
        Mock<ISessionProvider> mockSessionProvider = new();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.MockAs<AzureAppSettings>((t) => t.IsSessionIdStoredInCookie = false);
        Mock<ICookieProvider> mockCookieProvider = new();

        ConsentController controller = new(
            mockSessionProvider.Object,
            options,
            mockCookieProvider.Object);

        controller.StubHttpContext();

        // Act
        IActionResult result = controller.Index();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        ConsentViewModel? viewModel = Assert.IsType<ConsentViewModel>(viewResult.Model);
        Assert.NotNull(viewModel);

        mockCookieProvider.Verify(
            t => t.Set(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<CookieOptions>()),
            Times.Never);
    }

    [Fact]
    public void PostIndex_ReturnViewResultError_When_ConsentNotGiven()
    {
        // Arrange
        Mock<ISessionProvider> mockSessionProvider = new();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.Default<AzureAppSettings>();
        Mock<ICookieProvider> mockCookieProvider = new();

        ConsentController controller = new(
            mockSessionProvider.Object,
            options,
            mockCookieProvider.Object);

        ConsentViewModel consentModel = new()
        {
            ConsentGiven = false
        };

        // Act
        IActionResult result = controller.Index(consentModel);

        // Assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        ConsentViewModel viewModel = Assert.IsType<ConsentViewModel>(viewResult.Model);
        Assert.NotNull(viewModel);
        Assert.False(viewModel.ConsentGiven);
        Assert.True(viewModel.HasError);

        // Ensure session is not set
        mockSessionProvider.Verify(
            s => s.SetSessionValue(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void PostIndex_RedirectsToHome_When_ConsentGiven()
    {
        // Arrange
        Mock<ISessionProvider> mockSessionProvider = new();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.Default<AzureAppSettings>();
        Mock<ICookieProvider> mockCookieProvider = new();

        ConsentController controller = new(
            mockSessionProvider.Object,
            options,
            mockCookieProvider.Object);

        controller.StubHttpContext();

        ConsentViewModel consentModel = new()
        {
            ConsentGiven = true
        };

        // Act
        IActionResult result = controller.Index(consentModel);

        // Assert
        Assert.IsType<RedirectResult>(result);
        RedirectResult? redirectResult = result as RedirectResult;
        Assert.NotNull(redirectResult);
        Assert.Equal(Routes.Application.Home, redirectResult.Url);

        // Ensure session is set
        mockSessionProvider.Verify(
            s => s.SetSessionValue(SessionKeys.ConsentGivenKey, true), Times.Once);
    }
}
