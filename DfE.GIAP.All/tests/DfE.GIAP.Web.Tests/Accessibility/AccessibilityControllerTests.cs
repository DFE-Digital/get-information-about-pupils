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
using DfE.GIAP.Web.Tests.TestDoubles;

namespace DfE.GIAP.Web.Tests.Accessibility;

[Trait("Category", "Accessibility Controller Unit Tests")]
public sealed class AccessibilityControllerTests
{

    [Fact]
    public void AccessibilityController_Throws_When_ConstructedWithNullArgs_UseCase()
    {
        // Arrange
        Mock<IMapper<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>> mockMapper =
            MapperTestDoubles.Default<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>();

        // Act
        Func<AccessibilityController> construct = () => new AccessibilityController(null!, mockMapper.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void AccessibilityController_Throws_When_ConstructedWithNullArgs_Mapper()
    {
        // Arrange
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> _mockUseCase = new();
        Func<AccessibilityController> construct = () => new AccessibilityController(_mockUseCase.Object, null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task AccessibilityController_Index_Throws_When_ResponseContentIsNull()
    {
        // Arrange
        GetContentByPageKeyUseCaseResponse response = new(Content: null);

        Mock<IMapper<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>> mockMapper =
            MapperTestDoubles.MockFor<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>();

        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        mockUseCase.Setup(t => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()))
            .ReturnsAsync(response)
            .Verifiable();

        AccessibilityController sut = new(mockUseCase.Object, mockMapper.Object);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => _ = await sut.Index());

        mockUseCase.Verify(
            (t) => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()), Times.Once);

        mockMapper.Verify(t => t.Map(It.IsAny<GetContentByPageKeyUseCaseResponse>()), Times.Never);
    }

    [Fact]
    public async Task AccessibilityController_Index_Returns_ViewModel_When_ContentReceived()
    {
        // Arrange
        Content content = ContentTestDoubles.Default();

        GetContentByPageKeyUseCaseResponse response = new(content);

        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        mockUseCase.Setup(
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

        AccessibilityController sut = new(mockUseCase.Object, mockMapper.Object);

        // Act
        IActionResult result = await sut.Index();

        // Assert
        mockUseCase.Verify(
            (t) => t.HandleRequestAsync(
                It.IsAny<GetContentByPageKeyUseCaseRequest>()), Times.Once);

        mockMapper.Verify(
            (t) => t.Map(
                It.IsAny<GetContentByPageKeyUseCaseResponse>()), Times.Once);

        ViewResult viewResult = Assert.IsType<ViewResult>(result, exactMatch: false);
        Assert.NotNull(viewResult);

        AccessibilityViewModel? viewModel = viewResult.Model as AccessibilityViewModel;
        Assert.NotNull(viewModel);
        Assert.Equal(content.Title, viewModel.Response.Title);
        Assert.Equal(content.Body, viewModel.Response.Body);
    }
}
