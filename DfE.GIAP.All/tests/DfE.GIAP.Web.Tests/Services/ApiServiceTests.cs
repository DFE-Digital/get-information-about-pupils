using System.Net;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using DfE.GIAP.Service.Tests.FakeLogger;
using DfE.GIAP.Service.Tests.Models;
using DfE.GIAP.Web.Services.ApiProcessor;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Tests.Services;

public sealed class ApiServiceTests
{
    [Fact]
    public async Task GetAsync_with_response_model_returns_correct_type()
    {
        // arrange
        var expected = new ApiItemModel();
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        var fakeLogger = new Mock<ILogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        var actual = await apiProcessorService.GetAsync<ApiItemModel>(url);

        // assert
        Assert.IsType<ApiItemModel>(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetAsync_logs_exception_and_returns_default()
    {
        // arrange
        ApiItemModel expected = null;
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        var fakeLogger = new Mock<ILogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);
        var fakeException = new HttpRequestException();
        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Throws(fakeException);


        // act
        var actual = await apiProcessorService.GetAsync<ApiItemModel>(url);

        // assert
        Assert.Null(actual);
        Assert.Equal(expected, actual);

        fakeLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(lvl => lvl == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(ex => ex == fakeException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

    }


    [Fact]
    public async Task GetAsync_logs_exception_and_retuns_default()
    {
        // arrange
        List<ApiItemModel> expected = null;
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage();
        var fakeLogger = new Mock<FakeLogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Throws(new HttpRequestException());

        // act
        var actual = await apiProcessorService.GetAsync<List<ApiItemModel>>(url);

        // assert
        Assert.Null(actual);
        Assert.Equal(expected, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Once);

    }

    [Fact]
    public async Task GetAsync_doesnt_log_404s()
    {
        // arrange
        List<ApiItemModel> expected = null;
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound
        };
        var fakeLogger = new Mock<FakeLogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        var actual = await apiProcessorService.GetAsync<List<ApiItemModel>>(url);

        // assert
        Assert.Null(actual);
        Assert.Equal(expected, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Never);
    }

    [Fact]
    public async Task GetToListAsync_returns_list_of_response_model()
    {
        // arrange
        var expected = new List<ApiItemModel>();
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        var fakeLogger = new Mock<ILogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        var actual = await apiProcessorService.GetToListAsync<ApiItemModel>(url);

        // assert
        Assert.IsType<List<ApiItemModel>>(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetToListAsync_logs_exception_and_returns_default()
    {
        // arrange
        List<ApiItemModel> expected = null;
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        var fakeLogger = new Mock<FakeLogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);
        var fakeException = new HttpRequestException();
        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Throws(fakeException);

        // act
        var actual = await apiProcessorService.GetToListAsync<ApiItemModel>(url);

        // assert
        Assert.Null(actual);
        Assert.Equal(expected, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.Is<HttpRequestException>(e => e.Equals(fakeException))), Times.Once);
    }

    [Fact]
    public async Task GetToListAsync_logs_error_and_returns_default()
    {
        // arrange
        List<ApiItemModel> expected = null;
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadGateway
        };
        var fakeLogger = new Mock<FakeLogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);


        // act
        var actual = await apiProcessorService.GetToListAsync<ApiItemModel>(url);

        // assert
        Assert.Null(actual);
        Assert.Equal(expected, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Once);
    }

    [Fact]
    public async Task GetToListAsync_doesnt_log_404s()
    {
        // arrange
        List<ApiItemModel> expected = null;
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound
        };
        var fakeLogger = new Mock<FakeLogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        var actual = await apiProcessorService.GetToListAsync<ApiItemModel>(url);

        // assert
        Assert.Null(actual);
        Assert.Equal(expected, actual);

        fakeLogger.Verify(t => t.Log(
            LogLevel.Error,
            It.IsAny<string>(),
            It.IsAny<HttpRequestException>()), Times.Never);
    }

    [Fact]
    public async Task PostAsync_with_model_returns_good_status_code()
    {
        // arrange
        var expected = new List<ApiItemModel>();
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };
        var fakeLogger = new Mock<FakeLogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        var actual = await apiProcessorService.PostAsync<List<ApiItemModel>>(url, expected);

        // assert
        Assert.IsType<HttpStatusCode>(actual);
        Assert.Equal(HttpStatusCode.OK, actual);
    }

    [Fact]
    public async Task PostAsync_with_model_logs_error_and_returns_bad_status_code()
    {
        // arrange
        var expected = new List<ApiItemModel>();
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadGateway
        };
        var fakeLogger = new Mock<FakeLogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        var actual = await apiProcessorService.PostAsync<List<ApiItemModel>>(url, expected);

        // assert
        Assert.IsType<HttpStatusCode>(actual);
        Assert.Equal(HttpStatusCode.BadGateway, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Once);
    }

    [Fact]
    public async Task PostAsync_with_model_logs_error_after_exception_returns_bad_status_code()
    {
        // arrange
        var expected = new List<ApiItemModel>();
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage();
        var fakeLogger = new Mock<FakeLogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Throws(new HttpRequestException());
        // act
        var actual = await apiProcessorService.PostAsync<List<ApiItemModel>>(url, expected);

        // assert
        Assert.IsType<HttpStatusCode>(actual);
        Assert.Equal(HttpStatusCode.BadRequest, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Once);
    }

    [Fact]
    public async Task PostAsync_with_model_logs_error_doesnt_log_404s()
    {
        // arrange
        var expected = new List<ApiItemModel>();
        var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound
        };
        var fakeLogger = new Mock<FakeLogger<ApiService>>();

        var apiProcessorService = new ApiService(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        var actual = await apiProcessorService.PostAsync<List<ApiItemModel>>(url, expected);

        // assert
        Assert.IsType<HttpStatusCode>(actual);
        Assert.Equal(HttpStatusCode.NotFound, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Never);

    }
}
