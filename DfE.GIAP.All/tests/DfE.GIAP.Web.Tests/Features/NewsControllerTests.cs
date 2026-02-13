using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features;
using DfE.GIAP.Web.Providers.Session;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features;

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
    public async Task Index_SetsSessionValue_WhenSessionKeyExists()
    {
        // Arrange
        List<NewsArticle> articles = new();
        GetNewsArticlesResponse response = new(articles);
        Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>> useCase = new Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>>();
        useCase.Setup(x => x.HandleRequestAsync(It.IsAny<GetNewsArticlesRequest>()))
               .ReturnsAsync(response);

        Mock<ISessionProvider> sessionProvider = new Mock<ISessionProvider>();
        sessionProvider.Setup(x => x.ContainsSessionKey(SessionKeys.ShowNewsBannerKey)).Returns(true);

        NewsController controller = new(sessionProvider.Object, useCase.Object);

        // Act
        await controller.Index();

        // Assert
        sessionProvider.Verify(x => x.SetSessionValue(SessionKeys.ShowNewsBannerKey, false), Times.Once);
    }


    [Fact]
    public void DismissNewsBanner_SetsSessionKeyAndRedirectsToLocalUrl()
    {
        // Arrange
        Mock<ISessionProvider> sessionProvider = new();
        sessionProvider.Setup(x => x.ContainsSessionKey(SessionKeys.ShowNewsBannerKey)).Returns(true);
        Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>> useCase = new Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>>();
        NewsController controller = new(sessionProvider.Object, useCase.Object);

        Mock<IUrlHelper> urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(x => x.IsLocalUrl("/local")).Returns(true);
        controller.Url = urlHelper.Object;

        // Act
        IActionResult result = controller.DismissNewsBanner("/local");

        // Assert
        sessionProvider.Verify(x => x.SetSessionValue(SessionKeys.ShowNewsBannerKey, false), Times.Once);
        RedirectResult redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/local", redirectResult.Url);
    }

    [Fact]
    public void DismissNewsBanner_RedirectsToHomeIndex_WhenUrlIsNotLocalOrNull()
    {
        // Arrange
        Mock<ISessionProvider> sessionProvider = new();
        sessionProvider.Setup(x => x.ContainsSessionKey(SessionKeys.ShowNewsBannerKey)).Returns(true);
        Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>> useCase = new Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>>();
        NewsController controller = new(sessionProvider.Object, useCase.Object);

        Mock<IUrlHelper> urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(x => x.IsLocalUrl(It.IsAny<string>())).Returns(false);
        controller.Url = urlHelper.Object;

        // Act
        IActionResult result = controller.DismissNewsBanner(null);

        // Assert
        sessionProvider.Verify(x => x.SetSessionValue(SessionKeys.ShowNewsBannerKey, false), Times.Once);
        RedirectToActionResult redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        Assert.Equal("Home", redirectToActionResult.ControllerName);
    }
}
