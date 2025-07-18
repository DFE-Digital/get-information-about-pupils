using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Privacy;

[Trait("Category", "Privacy Controller Unit Tests")]
public sealed class PrivacyControllerTests
{
    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_UseCase_IsNull()
    {
        // Arrange
        Mock<IMapper<GetContentByPageKeyUseCaseResponse, PrivacyViewModel>> mockMapper =
            MapperTestDoubles.Default<GetContentByPageKeyUseCaseResponse, PrivacyViewModel>();

        // Act
        Func<PrivacyController> construct = () => new PrivacyController(null!, mockMapper.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_Mapper_IsNull()
    {
        // Arrange
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> _mockUseCase = new();
        Func<PrivacyController> construct = () => new PrivacyController(_mockUseCase.Object, null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task Index_Throws_ArgumentException_When_ResponseContentIsNull()
    {
        // Arrange
        GetContentByPageKeyUseCaseResponse response = new(Content: null);

        Mock<IMapper<GetContentByPageKeyUseCaseResponse, PrivacyViewModel>> mockMapper =
            MapperTestDoubles.MockFor<GetContentByPageKeyUseCaseResponse, PrivacyViewModel>();

        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockUseCase = new();
        mockUseCase.Setup(t => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()))
            .ReturnsAsync(response)
            .Verifiable();

        PrivacyController sut = new(mockUseCase.Object, mockMapper.Object);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => _ = await sut.Index());

        mockUseCase.Verify(
            (t) => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()), Times.Once);

        mockMapper.Verify(t => t.Map(It.IsAny<GetContentByPageKeyUseCaseResponse>()), Times.Never);
    }

    [Fact]
    public async Task Index_Returns_ViewModel_When_ContentReceived()
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

        Mock<IMapper<GetContentByPageKeyUseCaseResponse, PrivacyViewModel>> mockMapper =
            MapperTestDoubles.MockFor<GetContentByPageKeyUseCaseResponse, PrivacyViewModel>(
                new PrivacyViewModel()
                {
                    Response = content
                });

        PrivacyController sut = new(mockUseCase.Object, mockMapper.Object);

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

        PrivacyViewModel? viewModel = viewResult.Model as PrivacyViewModel;
        Assert.NotNull(viewModel);
        Assert.Equal(content.Title, viewModel.Response.Title);
        Assert.Equal(content.Body, viewModel.Response.Body);
    }
}
