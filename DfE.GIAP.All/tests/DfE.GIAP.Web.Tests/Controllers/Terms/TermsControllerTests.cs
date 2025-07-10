using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Terms;

[Trait("Category", "Terms Controller Unit Tests")]
public class TermsControllerTests
{
    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_UseCase_IsNull()
    {
        // Arrange
        Mock<IMapper<GetContentByPageKeyResponse, TermsOfUseViewModel>> mockMapper =
            MapperTestDoubles.Default<GetContentByPageKeyResponse, TermsOfUseViewModel>();

        // Act
        Func<TermsController> construct = () => new TermsController(null!, mockMapper.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_Mapper_IsNull()
    {
        // Arrange
        Mock<IUseCase<GetContentByPageKeyRequest, GetContentByPageKeyResponse>> _mockUseCase = new();
        Func<TermsController> construct = () => new TermsController(_mockUseCase.Object, null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task Index_Throws_ArgumentException_When_ResponseContentIsNull()
    {
        // Arrange
        GetContentByPageKeyResponse response = new(Content: null);

        Mock<IMapper<GetContentByPageKeyResponse, TermsOfUseViewModel>> mockMapper =
            MapperTestDoubles.MockFor<GetContentByPageKeyResponse, TermsOfUseViewModel>();

        Mock<IUseCase<GetContentByPageKeyRequest, GetContentByPageKeyResponse>> mockUseCase = new();
        mockUseCase.Setup(t => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyRequest>()))
            .ReturnsAsync(response)
            .Verifiable();

        TermsController sut = new(mockUseCase.Object, mockMapper.Object);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => _ = await sut.Index());

        mockUseCase.Verify(
            (t) => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyRequest>()), Times.Once);

        mockMapper.Verify(t => t.Map(It.IsAny<GetContentByPageKeyResponse>()), Times.Never);
    }

    [Fact]
    public async Task Index_Returns_ViewModel_When_ContentReceived()
    {
        // Arrange
        Content content = ContentTestDoubles.Default();

        GetContentByPageKeyResponse response = new(content);

        Mock<IUseCase<GetContentByPageKeyRequest, GetContentByPageKeyResponse>> mockUseCase = new();
        mockUseCase.Setup(
            (t) => t.HandleRequestAsync(
                It.IsAny<GetContentByPageKeyRequest>()))
            .ReturnsAsync(response)
            .Verifiable();

        Mock<IMapper<GetContentByPageKeyResponse, TermsOfUseViewModel>> mockMapper =
            MapperTestDoubles.MockFor<GetContentByPageKeyResponse, TermsOfUseViewModel>(
                new TermsOfUseViewModel()
                {
                    Response = content
                });

        TermsController sut = new(mockUseCase.Object, mockMapper.Object);

        // Act
        IActionResult result = await sut.Index();

        // Assert
        mockUseCase.Verify(
            (t) => t.HandleRequestAsync(
                It.IsAny<GetContentByPageKeyRequest>()), Times.Once);

        mockMapper.Verify(
            (t) => t.Map(
                It.IsAny<GetContentByPageKeyResponse>()), Times.Once);

        ViewResult viewResult = Assert.IsType<ViewResult>(result, exactMatch: false);
        Assert.NotNull(viewResult);

        TermsOfUseViewModel? viewModel = viewResult.Model as TermsOfUseViewModel;
        Assert.NotNull(viewModel);
        Assert.Equal(content.Title, viewModel.Response.Title);
        Assert.Equal(content.Body, viewModel.Response.Body);
    }
}
