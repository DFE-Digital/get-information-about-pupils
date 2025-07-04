using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Core.Models.News;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.News;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace DfE.GIAP.Service.Tests.Services.News;

[Trait("Category", "News Service Unit Tests")]
public class NewsServiceTests
{
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
