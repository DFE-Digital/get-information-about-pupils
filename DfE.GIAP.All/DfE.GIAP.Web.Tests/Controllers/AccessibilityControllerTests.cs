using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using DfE.GIAP.Core.SharedTests.TestDoubles;

namespace DfE.GIAP.Web.Tests.Controllers;

[Trait("Category", "Accessibility Controller Unit Tests")]
public sealed class AccessibilityControllerTests
{
    private readonly Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> _mockUseCase = new();

#pragma warning disable CA1806 // Do not ignore method results
    [Fact]
    public void AccessibilityController_Throws_When_ConstructedWithNullArgs_UseCase()
    {
        Mock<IMapper<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>> mockMapper =
            MapperTestDoubles.Default<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>();

        Action construct = () => new AccessibilityController(null!, mockMapper.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void AccessibilityController_Throws_When_ConstructedWithNullArgs_Mapper()
    {
        Action construct = () => new AccessibilityController(_mockUseCase.Object, null!);
        Assert.Throws<ArgumentNullException>(construct);
    }
#pragma warning restore CA1806 // Do not ignore method results


    [Fact]
    public async Task AccessibilityController_Index_Throws_When_ResponseContentIsNull()
    {
        GetContentByPageKeyUseCaseResponse response = new(Content: null);

        Mock<IMapper<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>> mockMapper =
            MapperTestDoubles.MockFor<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>();

        _mockUseCase.Setup(t => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()))
            .ReturnsAsync(response)
            .Verifiable();

        AccessibilityController sut = new(_mockUseCase.Object, mockMapper.Object);

        await Assert.ThrowsAsync<ArgumentException>(async () => _ = await sut.Index());

        _mockUseCase.Verify(
            (t) => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()), Times.Once);

        mockMapper.Verify(t => t.Map(It.IsAny<GetContentByPageKeyUseCaseResponse>()), Times.Never);
    }

    [Fact]
    public async Task AccessibilityController_Index_Returns_ViewModel_When_ContentReceived()
    {
        Content content = ContentTestDoubles.Default();

        GetContentByPageKeyUseCaseResponse response = new(content);

        _mockUseCase.Setup(
            (t) => t.HandleRequestAsync(
                It.IsAny<GetContentByPageKeyUseCaseRequest>()))
            .ReturnsAsync(response)
            .Verifiable();


        Mock<IMapper<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>> mockMapper =
            MapperTestDoubles.MockFor<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>(
                new AccessibilityViewModel()
                {
                    Response = content
                });

        AccessibilityController sut = new(_mockUseCase.Object, mockMapper.Object);

        IActionResult result = await sut.Index();

        // Assert
        _mockUseCase.Verify(
            (t) => t.HandleRequestAsync(
                It.IsAny<GetContentByPageKeyUseCaseRequest>()), Times.Once);

        mockMapper.Verify(
            (t) => t.Map(
                It.IsAny<GetContentByPageKeyUseCaseResponse>()), Times.Once());

        ViewResult viewResult = Assert.IsType<ViewResult>(result, exactMatch: false);
        Assert.NotNull(viewResult);

        AccessibilityViewModel? viewModel = viewResult.Model as AccessibilityViewModel;
        Assert.NotNull(viewModel);
        Assert.Equal(content.Title, viewModel.Response.Title);
        Assert.Equal(content.Body, viewModel.Response.Body);
    }

    [Fact]
    public async Task AccessibilityController_Report_Throws_When_ResponseContentIsNull()
    {
        GetContentByPageKeyUseCaseResponse response = new(Content: null);

        Mock<IMapper<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>> mockMapper =
            MapperTestDoubles.MockFor<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>();

        _mockUseCase.Setup(t => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()))
            .ReturnsAsync(response)
            .Verifiable();

        AccessibilityController sut = new(_mockUseCase.Object, mockMapper.Object);

        await Assert.ThrowsAsync<ArgumentException>(async () => _ = await sut.Report());

        _mockUseCase.Verify(
            (t) => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()), Times.Once);

        mockMapper.Verify(t => t.Map(It.IsAny<GetContentByPageKeyUseCaseResponse>()), Times.Never);
    }

    [Fact]
    public async Task AccessibilityController_Report_Returns_ViewModel_When_ContentReceived()
    {
        Content content = ContentTestDoubles.Default();

        GetContentByPageKeyUseCaseResponse response = new(content);

        _mockUseCase.Setup(
            (t) => t.HandleRequestAsync(
                It.IsAny<GetContentByPageKeyUseCaseRequest>()))
            .ReturnsAsync(response)
            .Verifiable();


        Mock<IMapper<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>> mockMapper =
            MapperTestDoubles.MockFor<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>(
                new AccessibilityViewModel()
                {
                    Response = content
                });

        AccessibilityController sut = new(_mockUseCase.Object, mockMapper.Object);

        IActionResult result = await sut.Report();

        // Assert
        _mockUseCase.Verify(
            (t) => t.HandleRequestAsync(
                It.IsAny<GetContentByPageKeyUseCaseRequest>()), Times.Once);

        mockMapper.Verify(
            (t) => t.Map(
                It.IsAny<GetContentByPageKeyUseCaseResponse>()), Times.Once());

        ViewResult viewResult = Assert.IsType<ViewResult>(result, exactMatch: false);
        Assert.NotNull(viewResult);

        AccessibilityViewModel? viewModel = viewResult.Model as AccessibilityViewModel;
        Assert.NotNull(viewModel);
        Assert.Equal(content.Title, viewModel.Response.Title);
        Assert.Equal(content.Body, viewModel.Response.Body);
    }
}


// TODO generate with Faker
internal static class ContentTestDoubles
{
    internal static Content Default() => new()
    {
        Body = "Test body $%£%\"@{ \t \r \r\n }~",
        Title = "Test $%£^£ £\" \' \t \r \r\n title"
    };
}
