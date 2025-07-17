using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Web.Helpers.CookieManager;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Consent;

public sealed class ConsentControllerTests
{
    [Fact]
    public void Constructor_Throws_When_ConstructedWithNullSessionProvider()
    {
        // Arrange
        IOptions<AzureAppSettings> options = OptionsTestDoubles.Default<AzureAppSettings>();
        Mock<ICookieManager> mockCookieManager = new();
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        Mock<IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel>> mockMapper = new();

        // Act
        Func<ConsentController> construct = () => new ConsentController(
            null!,
            options,
            mockCookieManager.Object,
            mockUseCase.Object,
            mockMapper.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_ConstructedWithNullOptions()
    {
        // Arrange
        var mockSessionProvider = new Mock<ISessionProvider>();
        Mock<ICookieManager> mockCookieManager = new();
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        Mock<IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel>> mockMapper = new();

        // Act
        Func<ConsentController> construct = () => new ConsentController(
            mockSessionProvider.Object,
            null!,
            mockCookieManager.Object,
            mockUseCase.Object,
            mockMapper.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_ConstructedWithNullOptionsValue()
    {
        // Arrange
        var mockSessionProvider = new Mock<ISessionProvider>();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.ConfigureOptionsWithNullValue<AzureAppSettings>();
        Mock<ICookieManager> mockCookieManager = new();
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        Mock<IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel>> mockMapper = new();

        // Act
        Func<ConsentController> construct = () => new ConsentController(
            mockSessionProvider.Object,
            options,
            mockCookieManager.Object,
            mockUseCase.Object,
            mockMapper.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_ConstructedWithNullCookieManager()
    {
        // Arrange
        var mockSessionProvider = new Mock<ISessionProvider>();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.Default<AzureAppSettings>();
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        Mock<IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel>> mockMapper = new();

        // Act
        Func<ConsentController> construct = () => new ConsentController(
            mockSessionProvider.Object,
            options,
            null!,
            mockUseCase.Object,
            mockMapper.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_ConstructedWithNullUseCase()
    {
        // Arrange
        var mockSessionProvider = new Mock<ISessionProvider>();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.Default<AzureAppSettings>();
        Mock<IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel>> mockMapper = new();
        Mock<ICookieManager> mockCookieManager = new();

        // Act
        Func<ConsentController> construct = () => new ConsentController(
            mockSessionProvider.Object,
            options,
            mockCookieManager.Object,
            null!,
            mockMapper.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_ConstructedWithNullMapper()
    {
        // Arrange
        var mockSessionProvider = new Mock<ISessionProvider>();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.Default<AzureAppSettings>();
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        Mock<ICookieManager> mockCookieManager = new();

        // Act
        Func<ConsentController> construct = () => new ConsentController(
            mockSessionProvider.Object,
            options,
            mockCookieManager.Object,
            mockUseCase.Object,
            null!);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task GetIndex_Throws_When_ContentIsNull()
    {
        // Arrange
        var mockSessionProvider = new Mock<ISessionProvider>();
        GetContentByPageKeyUseCaseResponse response = new(Content: null);
        IOptions<AzureAppSettings> options = OptionsTestDoubles.Default<AzureAppSettings>();
        Mock<IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel>> mockMapper = new();
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        mockUseCase.Setup(
                (t) => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()))
            .ReturnsAsync(response);

        Mock<ICookieManager> mockCookieManager = new();

        ConsentController controller = new(
            mockSessionProvider.Object,
            options,
            mockCookieManager.Object,
            mockUseCase.Object,
            mockMapper.Object);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentException>(() => controller.Index());
    }

    [Fact]
    public async Task GetIndex_Returns_ConsentView_And_SetsCookie_When_SessionIdStorageEnabled()
    {
        // Arrange
        var mockSessionProvider = new Mock<ISessionProvider>();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.ConfigureOptions<AzureAppSettings>((t) => t.IsSessionIdStoredInCookie = true);
        Content content = ContentTestDoubles.Default();
        GetContentByPageKeyUseCaseResponse response = new(content);
        Mock<ICookieManager> mockCookieManager = new();

        Mock<IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel>> mockMapper =
            MapperTestDoubles.MockFor<GetContentByPageKeyUseCaseResponse, ConsentViewModel>(
                new ConsentViewModel()
                {
                    Response = content
                });

        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        mockUseCase.Setup(
                (t) => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()))
            .ReturnsAsync(response);

        ConsentController controller = new(
            mockSessionProvider.Object,
            options,
            mockCookieManager.Object,
            mockUseCase.Object,
            mockMapper.Object);

        HttpContext context = controller.StubHttpContext();

        // Act
        IActionResult result = await controller.Index();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        ConsentViewModel? viewModel = Assert.IsType<ConsentViewModel>(viewResult.Model);
        Assert.NotNull(viewModel);
        Assert.Equal(content, viewModel.Response);

        mockCookieManager.Verify(
            (t) => t.Set(
                It.IsAny<string>(),
                context.User.GetSessionId(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<CookieOptions>()), Times.Once());

        mockUseCase.Verify(
            (t) => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()), Times.Once);
    }

    [Fact]
    public async Task GetIndex_Returns_ConsentView_And_DoesNotSetCookie_When_SessionIdStorageDisabled()
    {
        // Arrange
        var mockSessionProvider = new Mock<ISessionProvider>();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.ConfigureOptions<AzureAppSettings>((t) => t.IsSessionIdStoredInCookie = false);
        Content content = ContentTestDoubles.Default();
        GetContentByPageKeyUseCaseResponse response = new(content);
        Mock<ICookieManager> mockCookieManager = new();

        Mock<IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel>> mockMapper =
           MapperTestDoubles.MockFor<GetContentByPageKeyUseCaseResponse, ConsentViewModel>(
               new ConsentViewModel()
               {
                   Response = content
               });

        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        mockUseCase.Setup(
                (t) => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()))
            .ReturnsAsync(response);

        ConsentController controller = new(
            mockSessionProvider.Object,
            options,
            mockCookieManager.Object,
            mockUseCase.Object,
            mockMapper.Object);

        controller.StubHttpContext();

        // Act
        IActionResult result = await controller.Index();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        ConsentViewModel? viewModel = Assert.IsType<ConsentViewModel>(viewResult.Model);
        Assert.NotNull(viewModel);
        Assert.Equal(content, viewModel.Response);

        mockCookieManager.Verify(
            t => t.Set(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<CookieOptions>()),
            Times.Never);

        mockUseCase.Verify(
            (t) => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()), Times.Once);
    }

    [Fact]
    public void PostIndex_ReturnViewResultError_When_ConsentNotGiven()
    {
        // Arrange
        var mockSessionProvider = new Mock<ISessionProvider>();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.Default<AzureAppSettings>();
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        Mock<IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel>> mockMapper = new();
        Mock<ICookieManager> mockCookieManager = new();

        ConsentController controller = new(
            mockSessionProvider.Object,
            options,
            mockCookieManager.Object,
            mockUseCase.Object,
            mockMapper.Object);

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
        var mockSessionProvider = new Mock<ISessionProvider>();
        IOptions<AzureAppSettings> options = OptionsTestDoubles.Default<AzureAppSettings>();
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        Mock<IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel>> mockMapper = new();
        Mock<ICookieManager> mockCookieManager = new();

        ConsentController controller = new(
            mockSessionProvider.Object,
            options,
            mockCookieManager.Object,
            mockUseCase.Object,
            mockMapper.Object);

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
            s => s.SetSessionValue(SessionKeys.ConsentGiven, SessionKeys.ConsentValue), Times.Once);
    }
}
