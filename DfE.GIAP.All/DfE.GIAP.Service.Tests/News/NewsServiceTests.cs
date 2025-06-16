using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Core.Models.News;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.News;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Service.Tests.Services.News;

[Trait("Category", "News Service Unit Tests")]
public class NewsServiceTests
{
    [Fact]
    public async Task GetNewsArticlesReturnsListOfArticlesInPinnedAndModifiedDateOrder()
    {
        // arrange
        var article1 = new Article() { Body = "Test body 1", Pinned = false, ModifiedDate = new DateTime(2020, 1, 3) };
        var article3 = new Article() { Body = "Test body 2", Pinned = true, ModifiedDate = new DateTime(2020, 1, 1) };
        var article2 = new Article() { Body = "Test body 3", Pinned = true, ModifiedDate = new DateTime(2020, 1, 5) };
        var articles = new List<Article>() { article1, article2, article3 };
        var expected = articles.OrderByDescending(x => x.Pinned).ThenByDescending(x => x.ModifiedDate).ToList();

        var requestBody = new RequestBody() { ARCHIVED = false, DRAFTS = true };

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(articles)) };
        fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        var apiProcessorService = new ApiService(httpClient, null);

        var url = "https://www.somewhere.com";
        var urls = new AzureAppSettings() { QueryNewsArticlesUrl = url };
        var azureFunctionUrls = new Mock<IOptions<AzureAppSettings>>();
        azureFunctionUrls.SetupGet(x => x.Value).Returns(urls);
        var newsService = new NewsService(apiProcessorService, azureFunctionUrls.Object);

        // act
        var actual = await newsService.GetNewsArticles(requestBody);

        // assert
        Assert.IsType<List<Article>>(actual);
        Assert.Equal(expected.Count, actual.Count);
        Assert.Equal(expected[0].Date, actual[0].Date);
        Assert.Equal(expected[1].Date, actual[1].Date);
        Assert.Equal(expected[2].Date, actual[2].Date);
    }

    [Fact]
    public async Task GetArchivedNewsArticlesReturnsListOfArticlesInModifiedDateOrder()
    {
        // arrange
        var article1 = new Article() { Body = "Test body 1", Published = true, ModifiedDate = new DateTime(2020, 1, 3), Archived = true, Pinned = false };
        var article2 = new Article() { Body = "Test body 2", Published = true, ModifiedDate = new DateTime(2020, 1, 4), Archived = false, Pinned = true };
        var article3 = new Article() { Body = "Test body 3", Published = true, ModifiedDate = new DateTime(2020, 1, 2), Archived = true, Pinned = true };
        var article4 = new Article() { Body = "Test body 4", Published = true, ModifiedDate = new DateTime(2020, 1, 1), Archived = false, Pinned = false };
        var articles = new List<Article>() { article1, article2, article3, article4 };
        var expected = articles.OrderByDescending(x => x.ModifiedDate).ToList();

        var requestBody = new RequestBody() { ARCHIVED = true, DRAFTS = true };

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(articles)) };
        fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        var apiProcessorService = new ApiService(httpClient, null);

        var url = "https://www.somewhere.com";
        var urls = new AzureAppSettings() { QueryNewsArticlesUrl = url };
        var azureFunctionUrls = new Mock<IOptions<AzureAppSettings>>();
        azureFunctionUrls.SetupGet(x => x.Value).Returns(urls);
        var newsService = new NewsService(apiProcessorService, azureFunctionUrls.Object);

        // act
        var actual = await newsService.GetNewsArticles(requestBody);

        // assert
        fakeHttpRequestSender.Verify(x => x.Send(It.IsAny<HttpRequestMessage>()), Times.Once());

        Assert.IsType<List<Article>>(actual);
        Assert.Equal(expected.Count, actual.Count);
        Assert.Equal(expected[0].ModifiedDate, actual[0].ModifiedDate);
        Assert.Equal(expected[0].Body, actual[0].Body);
        Assert.Equal(expected[1].ModifiedDate, actual[1].ModifiedDate);
        Assert.Equal(expected[1].Body, actual[1].Body);
        Assert.Equal(expected[2].ModifiedDate, actual[2].ModifiedDate);
        Assert.Equal(expected[2].Body, actual[2].Body);
        Assert.Equal(expected[3].ModifiedDate, actual[3].ModifiedDate);
        Assert.Equal(expected[3].Body, actual[3].Body);
    }


    [Fact]
    public async Task UpdateNewsArticle()
    {
        // arrange
        var expected = new Article() { Body = "Test body 1", Date = new DateTime(2020, 1, 1), Id = "d7a5a5a4-e3d7-4026-9ce3-a54b33c7cd8e" };

        var requestBody = new UpdateNewsRequestBody() { ID = "d7a5a5a4-e3d7-4026-9ce3-a54b33c7cd8e", ACTION = 2 };

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        var apiProcessorService = new ApiService(httpClient, null);

        var url = "https://www.somewhere.com";
        var urls = new AzureAppSettings() { UpdateNewsPropertyUrl = url };
        var azureFunctionUrls = new Mock<IOptions<AzureAppSettings>>();
        azureFunctionUrls.SetupGet(x => x.Value).Returns(urls);
        var newsService = new NewsService(apiProcessorService, azureFunctionUrls.Object);

        // act
        var actual = await newsService.UpdateNewsArticle(requestBody);

        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Date, actual.Date);
    }

    [Fact]
    public async Task UpdateNewsDocument()
    {
        // arrange
        var expected = new Article() { Title = "Test Title", Date = new DateTime(2020, 1, 1), Id = "d7a5a5a4-e3d7-4026-9ce3-a54b33c7cd8e" };

        var requestBody = new UpdateNewsDocumentRequestBody() { Id = "d7a5a5a4-e3d7-4026-9ce3-a54b33c7cd8e", Title = "Test Title", Body = "Test Body", DocType = (int)NewsDocType.NewsArticles };

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        var apiProcessorService = new ApiService(httpClient, null);

        var url = "https://www.somewhere.com";
        var urls = new AzureAppSettings() { UpdateNewsDocumentUrl = url };
        var azureFunctionUrls = new Mock<IOptions<AzureAppSettings>>();
        azureFunctionUrls.SetupGet(x => x.Value).Returns(urls);
        var newsService = new NewsService(apiProcessorService, azureFunctionUrls.Object);

        // act
        var actual = await newsService.UpdateNewsDocument(requestBody);

        fakeHttpRequestSender.Verify(x => x.Send(It.IsAny<HttpRequestMessage>()), Times.Once());
        Assert.IsType<Article>(actual);
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Date, actual.Date);
    }

    [Fact]
    public async Task DeleteNewsArticle()
    {
        var id = "d7a5a5a4-e3d7-4026-9ce3-a54b33c7cd8e";

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK };
        fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        var apiProcessorService = new ApiService(httpClient, null);

        var url = "https://www.somewhere.com";
        var urls = new AzureAppSettings() { DeleteNewsArticleUrl = url };
        var azureFunctionUrls = new Mock<IOptions<AzureAppSettings>>();
        azureFunctionUrls.SetupGet(x => x.Value).Returns(urls);
        var newsService = new NewsService(apiProcessorService, azureFunctionUrls.Object);

        // act
        var actual = await newsService.DeleteNewsArticle(id);

        Assert.Equal("OK", actual.ToString());
    }


    [Fact]
    public async Task UpdateNewsProperty()
    {
        // arrange
        var expected = new Article() { Title = "Test Title", Date = new DateTime(2020, 1, 1), Id = "d7a5a5a4-e3d7-4026-9ce3-a54b33c7cd8e" };

        var requestBody = new UpdateNewsDocumentRequestBody() { Id = "d7a5a5a4-e3d7-4026-9ce3-a54b33c7cd8e", Title = "Test Title", Body = "Test Body", DocType = (int)NewsDocType.NewsArticles };

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        var apiProcessorService = new ApiService(httpClient, null);

        var url = "https://www.somewhere.com";
        var urls = new AzureAppSettings() { UpdateNewsPropertyUrl = url };
        var azureFunctionUrls = new Mock<IOptions<AzureAppSettings>>();
        azureFunctionUrls.SetupGet(x => x.Value).Returns(urls);
        var newsService = new NewsService(apiProcessorService, azureFunctionUrls.Object);

        // act
        var actual = await newsService.UpdateNewsProperty(requestBody);


        fakeHttpRequestSender.Verify(x => x.Send(It.IsAny<HttpRequestMessage>()), Times.Once());
        Assert.IsType<Article>(actual);
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Date, actual.Date);
    }
}
