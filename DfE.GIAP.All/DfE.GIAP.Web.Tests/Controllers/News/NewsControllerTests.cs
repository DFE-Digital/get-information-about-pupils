using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.News;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.News;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using DfE.GIAP.Common.AppSettings;
using Microsoft.Extensions.Options;
using NSubstitute;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Web.Tests.Controllers.News
{
    [Trait("Category", "News Controller Unit Tests")]
    public class NewsControllerTests
    {
        private readonly IOptions<AzureAppSettings> _mockAzureAppSettings = Substitute.For<IOptions<AzureAppSettings>>();
        private readonly ILatestNewsBanner _mockNewsBanner = Substitute.For<ILatestNewsBanner>();


        [Fact]
        public async Task IndexReturnsAViewWithPublicationData()
        {
            // Arrange
            var listPublicationData = new CommonResponseBody() { Title = "Title 1", Body = "Test body 1", Date = new DateTime(2020, 1, 1) };
            var listMaintenanceData = new CommonResponseBody() { Title = "Title 2", Body = "Test body 1", Date = new DateTime(2020, 1, 1) };

            var articleData1 = new NewsArticle() { Id = "", DraftBody = "", DraftTitle = "", Title = "Title 1", Body = "Test body 1", };
            var articleData2 = new NewsArticle() { Id = "", DraftBody = "", DraftTitle = "", Title = "Title 2", Body = "Test body 2", };
            var listArticleData = new List<NewsArticle>() { articleData1, articleData2 };

            var mockRepo = new Mock<INewsService>();
            var mockContentService = new Mock<IContentService>();
            var mockLogger = new Mock<ILogger<NewsController>>();
            var mockUserService = new Mock<ICommonService>();
            var mockCookieManager = new Mock<ICookieManager>();

            var newsViewModel = new NewsViewModel();
            newsViewModel.NewsPublication = listPublicationData;
            newsViewModel.NewsMaintenance = listMaintenanceData;
            newsViewModel.Articles = listArticleData;

            mockContentService.Setup(repo => repo.GetContent(DocumentType.PublicationSchedule)).ReturnsAsync(listPublicationData);
            mockContentService.Setup(repo => repo.GetContent(DocumentType.PlannedMaintenance)).ReturnsAsync(listMaintenanceData);
            //mockRepo.Setup(repo => repo.GetNewsArticles(It.IsAny<RequestBody>())).ReturnsAsync(listArticleData);

            var controller = new NewsController(mockRepo.Object, mockContentService.Object, _mockNewsBanner, null);

            // Act
            var result = await controller.Index().ConfigureAwait(false);

            // Assert
            mockContentService.Verify(x => x.GetContent(DocumentType.PublicationSchedule), Times.Once());
            mockContentService.Verify(x => x.GetContent(DocumentType.PlannedMaintenance), Times.Once());
            //mockRepo.Verify(x => x.GetNewsArticles(It.IsAny<RequestBody>()), Times.Once());

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
            var archivedArticleData1 = new NewsArticle() { Id = "", DraftBody = "", DraftTitle = "", Title = "Article Title 1", Body = "Test body 1", ModifiedDate = new DateTime(2020, 1, 4), Archived = true };
            var archivedArticleData2 = new NewsArticle() { Id = "", DraftBody = "", DraftTitle = "", Title = "Article Title 2", Body = "Test body 2", ModifiedDate = new DateTime(2020, 1, 2), Archived = false };
            var listArchivedArticleData = new List<NewsArticle>() { archivedArticleData1, archivedArticleData2 };

            var mockRepo = new Mock<INewsService>();
            var mockLogger = new Mock<ILogger<NewsController>>();
            var mockUserService = new Mock<ICommonService>();
            var mockContentService = new Mock<IContentService>();
            var mockCookieManager = new Mock<ICookieManager>();
            var newsViewModel = new NewsViewModel();

            newsViewModel.Articles = listArchivedArticleData;

            //mockRepo.Setup(repo => repo.GetNewsArticles(It.IsAny<RequestBody>())).ReturnsAsync(listArchivedArticleData);

            var controller = new NewsController(mockRepo.Object, mockContentService.Object, _mockNewsBanner, null);

            // Act
            var result = await controller.Archive().ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var articleModel = Assert.IsType<NewsViewModel>(viewResult.ViewData.Model).Articles;

            //mockRepo.Verify(x => x.GetNewsArticles(It.IsAny<RequestBody>()), Times.Once());

            Assert.Equal("Article Title 1", articleModel[0].Title);
            Assert.Equal("Test body 1", articleModel[0].Body);
            Assert.True(articleModel[0].Archived);

            Assert.Equal("Article Title 2", articleModel[1].Title);
            Assert.Equal("Test body 2", articleModel[1].Body);
            Assert.False(articleModel[1].Archived);

            Assert.Equal(2, articleModel.Count);
        }

        [Fact]
        public async Task DismissNewsBanner_redirects_to_ProvidedURL()
        {
            // Arrange
            var mockRepo = new Mock<INewsService>();
            var mockLogger = new Mock<ILogger<NewsController>>();
            var mockUserService = new Mock<ICommonService>();
            var mockContentService = new Mock<IContentService>();
            var mockCookieManager = new Mock<ICookieManager>();

            var controller = new NewsController(mockRepo.Object, mockContentService.Object, _mockNewsBanner, null);

            // Act
            var result = await controller.DismissNewsBanner("testURL");

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectResult>(result);
            Assert.True(viewResult.Url.Equals("testURL?returnToSearch=true"));
        }
    }
}
