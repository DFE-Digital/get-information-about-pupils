using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.News;

[Trait("Category", "News Controller Unit Tests")]
public class NewsControllerTests
{
    private readonly ILatestNewsBanner _mockNewsBanner = Substitute.For<ILatestNewsBanner>();
    private readonly Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>> _mockGetNewsArticlesUseCase = new();

    [Fact]
    public async Task IndexReturnsAViewWithPublicationData()
    {
        // Arrange
        var listPublicationData = new CommonResponseBody() { Title = "Title 1", Body = "Test body 1", Date = new DateTime(2020, 1, 1) };
        var listMaintenanceData = new CommonResponseBody() { Title = "Title 2", Body = "Test body 1", Date = new DateTime(2020, 1, 1) };

        var articleData1 = new NewsArticle() { Id = "1", Title = "Title 1", Body = "Test body 1", DraftTitle = string.Empty, DraftBody = string.Empty, CreatedDate = new DateTime(2020, 1, 1) };
        var articleData2 = new NewsArticle() { Id = "2", Title = "Title 2", Body = "Test body 2", DraftTitle = string.Empty, DraftBody = string.Empty, CreatedDate = new DateTime(2020, 1, 1) };
        var listArticleData = new List<NewsArticle>() { articleData1, articleData2 };

        var mockContentService = new Mock<IContentService>();

        var newsViewModel = new NewsViewModel();
        newsViewModel.NewsPublication = listPublicationData;
        newsViewModel.NewsMaintenance = listMaintenanceData;
        newsViewModel.NewsArticles = listArticleData;

        mockContentService.Setup(repo => repo.GetContent(DocumentType.PublicationSchedule)).ReturnsAsync(listPublicationData);
        mockContentService.Setup(repo => repo.GetContent(DocumentType.PlannedMaintenance)).ReturnsAsync(listMaintenanceData);
        _mockGetNewsArticlesUseCase.Setup(repo => repo.HandleRequest(It.IsAny<GetNewsArticlesRequest>()))
            .ReturnsAsync(new GetNewsArticlesResponse(listArticleData));

        var controller = new NewsController(mockContentService.Object, _mockNewsBanner, _mockGetNewsArticlesUseCase.Object);

        // Act
        var result = await controller.Index().ConfigureAwait(false);

        // Assert
        mockContentService.Verify(x => x.GetContent(DocumentType.PublicationSchedule), Times.Once());
        mockContentService.Verify(x => x.GetContent(DocumentType.PlannedMaintenance), Times.Once());
        _mockGetNewsArticlesUseCase.Verify(x => x.HandleRequest(It.IsAny<GetNewsArticlesRequest>()), Times.Once());

        var viewResult = Assert.IsType<ViewResult>(result);
        var publicationModel = Assert.IsType<NewsViewModel>(
            viewResult.ViewData.Model).NewsPublication;
        Assert.Equal("Test body 1", publicationModel.Body);

        var maintenanceModel = Assert.IsType<NewsViewModel>(
            viewResult.ViewData.Model).NewsMaintenance;
        Assert.Equal("Test body 1", maintenanceModel.Body);

    }

    [Fact]
    public async Task ReturnsAViewWithArchivedData()
    {
        // Arrange
        var archivedArticleData1 = new NewsArticle() { Id = "1", Title = "Title 1", Body = "Test body 1", DraftTitle = string.Empty, DraftBody = string.Empty, ModifiedDate = new DateTime(2020, 1, 4), Archived = true };
        var archivedArticleData2 = new NewsArticle() { Id = "2", Title = "Title 2", Body = "Test body 2", DraftTitle = string.Empty, DraftBody = string.Empty, ModifiedDate = new DateTime(2020, 1, 2), Archived = false };
        var listArchivedArticleData = new List<NewsArticle>() { archivedArticleData1, archivedArticleData2 };

        var mockContentService = new Mock<IContentService>();
        var newsViewModel = new NewsViewModel();

        newsViewModel.NewsArticles = listArchivedArticleData;

        _mockGetNewsArticlesUseCase.Setup(repo => repo.HandleRequest(It.IsAny<GetNewsArticlesRequest>()))
            .ReturnsAsync(new GetNewsArticlesResponse(listArchivedArticleData));

        var controller = new NewsController(mockContentService.Object, _mockNewsBanner, _mockGetNewsArticlesUseCase.Object);

        // Act
        var result = await controller.Archive().ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var articleModel = Assert.IsType<NewsViewModel>(viewResult.ViewData.Model).NewsArticles.ToList();

        _mockGetNewsArticlesUseCase.Verify(x => x.HandleRequest(It.IsAny<GetNewsArticlesRequest>()), Times.Once());

        Assert.Equal("Title 1", articleModel[0].Title);
        Assert.Equal("Test body 1", articleModel[0].Body);
        Assert.True(articleModel[0].Archived);

        Assert.Equal("Title 2", articleModel[1].Title);
        Assert.Equal("Test body 2", articleModel[1].Body);
        Assert.False(articleModel[1].Archived);

        Assert.Equal(2, articleModel.Count);
    }

    [Fact]
    public async Task DismissNewsBanner_redirects_to_ProvidedURL()
    {
        // Arrange
        var mockContentService = new Mock<IContentService>();

        var controller = new NewsController(mockContentService.Object, _mockNewsBanner, _mockGetNewsArticlesUseCase.Object);

        // Act
        var result = await controller.DismissNewsBanner("testURL");

        // Assert
        var viewResult = Assert.IsAssignableFrom<RedirectResult>(result);
        Assert.True(viewResult.Url.Equals("testURL?returnToSearch=true"));
    }
}
