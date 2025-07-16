using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.News;

[Trait("Category", "News Controller Unit Tests")]
public class NewsControllerTests
{

    [Fact]
    public async Task IndexReturnsAViewWithPublicationData()
    {
        // Arrange
        NewsArticle articleData1 = new()
        {
            Id = NewsArticleIdentifier.From("1"),
            Title = "Title 1",
            Body = "Test body 1",
            CreatedDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        NewsArticle articleData2 = new()
        {
            Id = NewsArticleIdentifier.From("2"),
            Title = "Title 2",
            Body = "Test body 2",
            CreatedDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        List<NewsArticle> listArticleData = [articleData1, articleData2];
        
        Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>> mockGetNewsArticlesUseCase = new();
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockGetContentByPageKeyUseCase = new();

        Content firstCallPublicationSchedule = ContentTestDoubles.Default();
        Content secondCallPlannedMaintenance = ContentTestDoubles.Default();

        mockGetContentByPageKeyUseCase.SetupSequence(
            (t) => t.HandleRequestAsync(
                    It.IsAny<GetContentByPageKeyUseCaseRequest>()))
                .ReturnsAsync(new GetContentByPageKeyUseCaseResponse(firstCallPublicationSchedule))
                .ReturnsAsync(new GetContentByPageKeyUseCaseResponse(secondCallPlannedMaintenance));

        mockGetNewsArticlesUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<GetNewsArticlesRequest>()))
            .ReturnsAsync(new GetNewsArticlesResponse(listArticleData));

        ILatestNewsBanner _mockNewsBanner = new Mock<ILatestNewsBanner>().Object;

        NewsController controller = new(_mockNewsBanner, mockGetNewsArticlesUseCase.Object, mockGetContentByPageKeyUseCase.Object);

        // Act
        IActionResult result = await controller.Index();

        // Assert
        mockGetNewsArticlesUseCase.Verify(x => x.HandleRequestAsync(It.IsAny<GetNewsArticlesRequest>()), Times.Once());
        mockGetContentByPageKeyUseCase.Verify(t => t.HandleRequestAsync(It.IsAny<GetContentByPageKeyUseCaseRequest>()), Times.Exactly(2));

        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Content publicationModel = Assert.IsType<NewsViewModel>(viewResult.ViewData.Model).NewsPublication;
        Assert.Equal(firstCallPublicationSchedule.Body, publicationModel.Body);

        Content maintenanceModel = Assert.IsType<NewsViewModel>(viewResult.ViewData.Model).NewsMaintenance;
        Assert.Equal(secondCallPlannedMaintenance.Body, maintenanceModel.Body);

    }


    [Fact]
    public async Task DismissNewsBanner_redirects_to_ProvidedURL()
    {
        // Arrange
        Mock<ILatestNewsBanner> mockNewsBanner = new();
        mockNewsBanner.Setup(t => t.RemoveLatestNewsStatus()).Verifiable();

        Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>> mockGetNewsArticlesUseCase = new();
        Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> mockGetContentByPageKeyUseCase = new();

        NewsController controller = new(mockNewsBanner.Object, mockGetNewsArticlesUseCase.Object, mockGetContentByPageKeyUseCase.Object);

        // Act
        IActionResult result = await controller.DismissNewsBanner("testURL");

        // Assert
        RedirectResult viewResult = Assert.IsType<RedirectResult>(result, exactMatch: false);
        Assert.Equal("testURL?returnToSearch=true", viewResult.Url);
        mockNewsBanner.Verify(t => t.RemoveLatestNewsStatus(), Times.Once);
    }
}
