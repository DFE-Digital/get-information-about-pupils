using System.Net;
using DfE.GIAP.Web.Services.ApiProcessor;
using DfE.GIAP.Web.Tests.Services.FakeHttpHandlers;
using DfE.GIAP.Web.Tests.Services.FakeLogger;
using DfE.GIAP.Web.Tests.Services.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Tests.Services;

public sealed class ApiServiceTests
{
    [Fact]
    public async Task GetAsync_with_response_model_returns_correct_type()
    {
        // arrange
        ApiItemModel expected = new();
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        using HttpClient httpClient = new(fakeHttpMessageHandler);
        using HttpResponseMessage httpResponse = new() { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        Mock<ILogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        ApiItemModel actual = await apiProcessorService.GetAsync<ApiItemModel>(url);

        // assert
        Assert.IsType<ApiItemModel>(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetAsync_logs_exception_and_returns_default()
    {
        // arrange
        ApiItemModel? expected = null;
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        using HttpClient httpClient = new(fakeHttpMessageHandler);
        using HttpResponseMessage httpResponse = new() { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        Mock<ILogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);
        HttpRequestException fakeException = new();
        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Throws(fakeException);


        // act
        ApiItemModel? actual = await apiProcessorService.GetAsync<ApiItemModel>(url);

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
        List<ApiItemModel>? expected = null;
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new();
        Mock<FakeLogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Throws(new HttpRequestException());

        // act
        List<ApiItemModel>? actual = await apiProcessorService.GetAsync<List<ApiItemModel>>(url);

        // assert
        Assert.Null(actual);
        Assert.Equal(expected, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Once);

    }

    [Fact]
    public async Task GetAsync_doesnt_log_404s()
    {
        // arrange
        List<ApiItemModel>? expected = null;
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new()
        {
            StatusCode = HttpStatusCode.NotFound
        };
        Mock<FakeLogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        List<ApiItemModel>? actual = await apiProcessorService.GetAsync<List<ApiItemModel>>(url);

        // assert
        Assert.Null(actual);
        Assert.Equal(expected, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Never);
    }

    [Fact]
    public async Task GetToListAsync_returns_list_of_response_model()
    {
        // arrange
        List<ApiItemModel> expected = new();
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new() { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        Mock<ILogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        List<ApiItemModel> actual = await apiProcessorService.GetToListAsync<ApiItemModel>(url);

        // assert
        Assert.IsType<List<ApiItemModel>>(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetToListAsync_logs_exception_and_returns_default()
    {
        // arrange
        List<ApiItemModel>? expected = null;
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new() { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        Mock<FakeLogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);
        HttpRequestException fakeException = new();
        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Throws(fakeException);

        // act
        List<ApiItemModel>? actual = await apiProcessorService.GetToListAsync<ApiItemModel>(url);

        // assert
        Assert.Null(actual);
        Assert.Equal(expected, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.Is<HttpRequestException>(e => e.Equals(fakeException))), Times.Once);
    }

    [Fact]
    public async Task GetToListAsync_logs_error_and_returns_default()
    {
        // arrange
        List<ApiItemModel>? expected = null;
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new()
        {
            StatusCode = HttpStatusCode.BadGateway
        };
        Mock<FakeLogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);


        // act
        List<ApiItemModel>? actual = await apiProcessorService.GetToListAsync<ApiItemModel>(url);

        // assert
        Assert.Null(actual);
        Assert.Equal(expected, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Once);
    }

    [Fact]
    public async Task GetToListAsync_doesnt_log_404s()
    {
        // arrange
        List<ApiItemModel>? expected = null;
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new()
        {
            StatusCode = HttpStatusCode.NotFound
        };
        Mock<FakeLogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        List<ApiItemModel>? actual = await apiProcessorService.GetToListAsync<ApiItemModel>(url);

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
        List<ApiItemModel> expected = new();
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new()
        {
            StatusCode = HttpStatusCode.OK
        };
        Mock<FakeLogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        HttpStatusCode actual = await apiProcessorService.PostAsync<List<ApiItemModel>>(url, expected);

        // assert
        Assert.IsType<HttpStatusCode>(actual);
        Assert.Equal(HttpStatusCode.OK, actual);
    }

    [Fact]
    public async Task PostAsync_with_model_logs_error_and_returns_bad_status_code()
    {
        // arrange
        List<ApiItemModel> expected = new();
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new()
        {
            StatusCode = HttpStatusCode.BadGateway
        };
        Mock<FakeLogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        HttpStatusCode actual = await apiProcessorService.PostAsync<List<ApiItemModel>>(url, expected);

        // assert
        Assert.IsType<HttpStatusCode>(actual);
        Assert.Equal(HttpStatusCode.BadGateway, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Once);
    }

    [Fact]
    public async Task PostAsync_with_model_logs_error_after_exception_returns_bad_status_code()
    {
        // arrange
        List<ApiItemModel> expected = new();
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new();
        Mock<FakeLogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Throws(new HttpRequestException());
        // act
        HttpStatusCode actual = await apiProcessorService.PostAsync<List<ApiItemModel>>(url, expected);

        // assert
        Assert.IsType<HttpStatusCode>(actual);
        Assert.Equal(HttpStatusCode.BadRequest, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Once);
    }

    [Fact]
    public async Task PostAsync_with_model_logs_error_doesnt_log_404s()
    {
        // arrange
        List<ApiItemModel> expected = new();
        Uri url = new("https://www.somewhere.com", UriKind.Absolute);

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new()
        {
            StatusCode = HttpStatusCode.NotFound
        };
        Mock<FakeLogger<ApiService>> fakeLogger = new();

        ApiService apiProcessorService = new(httpClient, fakeLogger.Object);

        fakeHttpRequestSender.Setup(t => t.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        // act
        HttpStatusCode actual = await apiProcessorService.PostAsync<List<ApiItemModel>>(url, expected);

        // assert
        Assert.IsType<HttpStatusCode>(actual);
        Assert.Equal(HttpStatusCode.NotFound, actual);
        fakeLogger.Verify(t => t.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Never);

    }
}
