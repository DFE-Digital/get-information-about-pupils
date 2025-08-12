using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Providers.Session;
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

        mockGetNewsArticlesUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<GetNewsArticlesRequest>()))
            .ReturnsAsync(new GetNewsArticlesResponse(listArticleData));

        ISessionProvider _sessionProvider = new Mock<ISessionProvider>().Object;

        NewsController controller = new(_sessionProvider, mockGetNewsArticlesUseCase.Object);

        // Act
        IActionResult result = await controller.Index();

        // Assert
        mockGetNewsArticlesUseCase.Verify(x => x.HandleRequestAsync(It.IsAny<GetNewsArticlesRequest>()), Times.Once());
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
    }


    [Fact]
    public void DismissNewsBanner_redirects_to_ProvidedURL()
    {
        // Arrange
        Mock<ISessionProvider> sessionProvider = new();
        Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>> mockGetNewsArticlesUseCase = new();
        NewsController controller = new(sessionProvider.Object, mockGetNewsArticlesUseCase.Object);

        // Act
        IActionResult result = controller.DismissNewsBanner("testURL");

        // Assert
        RedirectResult viewResult = Assert.IsType<RedirectResult>(result, exactMatch: false);
        Assert.Equal("testURL", viewResult.Url);
    }
}
