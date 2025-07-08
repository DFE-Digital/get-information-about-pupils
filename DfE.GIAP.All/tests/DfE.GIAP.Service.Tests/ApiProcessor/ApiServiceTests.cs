using DfE.GIAP.Common.Constants.AzureFunction;
using DfE.GIAP.Core.Models.News;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using DfE.GIAP.Service.Tests.FakeLogger;
using DfE.GIAP.Service.Tests.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Service.Tests.Services.ApiProcessor
{
    public class ApiServiceTests
    {
        [Fact]
        public async Task GetAsync_with_response_model_returns_correct_type()
        {
            // arrange
            var expected = new ApiItemModel();
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
            var fakeLogger = Substitute.For<ILogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

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
 
            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);
            var fakeException = new HttpRequestException();
            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(x => { throw fakeException; });

            // act
            var actual = await apiProcessorService.GetAsync<ApiItemModel>(url);

            // assert
            Assert.Null(actual);
            Assert.Equal(expected, actual);
            fakeLogger.Received().Log(LogLevel.Error, Arg.Any<string>(), Arg.Is<HttpRequestException>(fakeException));
        }


        [Fact]
        public async Task GetAsync_logs_exception_and_retuns_default()
        {
            // arrange
            List<ApiItemModel> expected = null;
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage();
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(x => { throw new HttpRequestException();  });

            // act
            var actual = await apiProcessorService.GetAsync<List<ApiItemModel>>(url);

            // assert
            Assert.Null(actual);
            Assert.Equal(expected, actual);
            fakeLogger.Received().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
        }

        [Fact]
        public async Task GetAsync_doesnt_log_404s()
        {
            // arrange
            List<ApiItemModel> expected = null;
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

            // act
            var actual = await apiProcessorService.GetAsync<List<ApiItemModel>>(url);

            // assert
            Assert.Null(actual);
            Assert.Equal(expected, actual);
            fakeLogger.DidNotReceive().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
        }

        [Fact]
        public async Task GetToListAsync_returns_list_of_response_model()
        {
            // arrange
            var expected = new List<ApiItemModel>();
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);
 
            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
            var fakeLogger = Substitute.For<ILogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

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

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);
            var fakeException = new HttpRequestException();
            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(x => { throw fakeException; });

            // act
            var actual = await apiProcessorService.GetToListAsync<ApiItemModel>(url);

            // assert
            Assert.Null(actual);
            Assert.Equal(expected, actual);
            fakeLogger.Received().Log(LogLevel.Error, Arg.Any<string>(), Arg.Is<HttpRequestException>(fakeException));
        }

        [Fact]
        public async Task GetToListAsync_logs_error_and_returns_default()
        {
            // arrange
            List<ApiItemModel> expected = null;
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadGateway
            };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

            // act
            var actual = await apiProcessorService.GetToListAsync<ApiItemModel>(url);

            // assert
            Assert.Null(actual);
            Assert.Equal(expected, actual);
            fakeLogger.Received().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
        }

        [Fact]
        public async Task GetToListAsync_doesnt_log_404s()
        {
            // arrange
            List<ApiItemModel> expected = null;
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

            // act
            var actual = await apiProcessorService.GetToListAsync<ApiItemModel>(url);

            // assert
            Assert.Null(actual);
            Assert.Equal(expected, actual);
            fakeLogger.DidNotReceive().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
        }

        [Fact]
        public async Task PostAsync_with_model_returns_good_status_code()
        {
            // arrange
            var expected = new List<ApiItemModel>();
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage 
            { 
                StatusCode = HttpStatusCode.OK
            };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

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

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadGateway
            };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

            // act
            var actual = await apiProcessorService.PostAsync<List<ApiItemModel>>(url, expected);

            // assert
            Assert.IsType<HttpStatusCode>(actual);
            Assert.Equal(HttpStatusCode.BadGateway, actual);
            fakeLogger.Received().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
        }

        [Fact]
        public async Task PostAsync_with_model_logs_error_after_exception_returns_bad_status_code()
        {
            // arrange
            var expected = new List<ApiItemModel>();
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage();
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(x => { throw new HttpRequestException();  });

            // act
            var actual = await apiProcessorService.PostAsync<List<ApiItemModel>>(url, expected);

            // assert
            Assert.IsType<HttpStatusCode>(actual);
            Assert.Equal(HttpStatusCode.BadRequest, actual);
            fakeLogger.Received().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
        }

        [Fact]
        public async Task PostAsync_with_model_logs_error_doesnt_log_404s()
        {
            // arrange
            var expected = new List<ApiItemModel>();
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

            // act
            var actual = await apiProcessorService.PostAsync<List<ApiItemModel>>(url, expected);

            // assert
            Assert.IsType<HttpStatusCode>(actual);
            Assert.Equal(HttpStatusCode.NotFound, actual);
            fakeLogger.DidNotReceive().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
        }

        [Fact]
        public async Task PostAsync_with_response_and_model_types_returns_correct_response_type()
        {
            // arrange
            var headerDetails = new AzureFunctionHeaderDetails()
            {
                ClientId = "test",
                SessionId = "testSesh"
            };

            var expected = new List<ApiItemModel>();
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);
            var requestBody = new RequestBody();

            var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };

            var apiProcessorService = new ApiService(httpClient, null);

            fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

            // act
            var actual = await apiProcessorService.PostAsync<RequestBody, List<ApiItemModel>>(url, requestBody, headerDetails);

            // assert
            Assert.IsType<List<ApiItemModel>>(actual);
            Assert.Equal(expected, actual);
            Assert.True(httpClient.DefaultRequestHeaders.GetValues(HeaderDetails.ClientId).SequenceEqual(new string[] { "test" }));
            Assert.True(httpClient.DefaultRequestHeaders.GetValues(HeaderDetails.SessionId).SequenceEqual(new string[] { "testSesh" }));
        }

        [Fact]
        public async Task PostAsync_with_response_and_model_types_logs_error_after_exception_and_returns_null()
        {
            // arrange
            var headerDetails = new AzureFunctionHeaderDetails()
            {
                ClientId = "test",
                SessionId = "testSesh"
            };

            List<ApiItemModel> expected = null;
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);
            var requestBody = new RequestBody();
            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage();
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(x => { throw new HttpRequestException(); });

            // act
            var actual = await apiProcessorService.PostAsync<RequestBody, List<ApiItemModel>>(url, requestBody, headerDetails);

            // assert
            Assert.Null(actual);
            Assert.Equal(expected, actual);
            fakeLogger.Received().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
            Assert.True(httpClient.DefaultRequestHeaders.GetValues(HeaderDetails.ClientId).SequenceEqual(new string[] { "test" }));
            Assert.True(httpClient.DefaultRequestHeaders.GetValues(HeaderDetails.SessionId).SequenceEqual(new string[] { "testSesh" }));
        }

        [Fact]
        public async Task PostAsync_with_response_and_model_types_logs_error_and_returns_null()
        {
            // arrange
            var headerDetails = new AzureFunctionHeaderDetails()
            {
                ClientId = "test",
                SessionId = "testSesh"
            };

            List<ApiItemModel> expected = null;
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);
            var requestBody = new RequestBody();
            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadGateway
            };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

            // act
            var actual = await apiProcessorService.PostAsync<RequestBody, List<ApiItemModel>>(url, requestBody, headerDetails);

            // assert
            Assert.Null(actual);
            Assert.Equal(expected, actual);
            fakeLogger.Received().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
            Assert.True(httpClient.DefaultRequestHeaders.GetValues(HeaderDetails.ClientId).SequenceEqual(new string[] { "test" }));
            Assert.True(httpClient.DefaultRequestHeaders.GetValues(HeaderDetails.SessionId).SequenceEqual(new string[] { "testSesh" }));
        }

        [Fact]
        public async Task PostAsync_with_response_and_model_types_doesnt_log_404()
        {
            // arrange
            var headerDetails = new AzureFunctionHeaderDetails()
            {
                ClientId = "test",
                SessionId = "testSesh"
            };

            List<ApiItemModel> expected = null;
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);
            var requestBody = new RequestBody();
            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

            // act
            var actual = await apiProcessorService.PostAsync<RequestBody, List<ApiItemModel>>(url, requestBody, headerDetails);

            // assert
            Assert.Null(actual);
            Assert.Equal(expected, actual);
            fakeLogger.DidNotReceive().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
            Assert.True(httpClient.DefaultRequestHeaders.GetValues(HeaderDetails.ClientId).SequenceEqual(new string[] { "test" }));
            Assert.True(httpClient.DefaultRequestHeaders.GetValues(HeaderDetails.SessionId).SequenceEqual(new string[] { "testSesh" }));
        }

        [Fact]
        public async Task PostAsync_with_response_and_model_types_logs_error_on_exception_and_returns_null()
        {
            // arrange
            var headerDetails = new AzureFunctionHeaderDetails()
            {
                ClientId = "test",
                SessionId = "testSesh"
            };

            List<ApiItemModel> expected = new List<ApiItemModel>(); 
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);
            var requestBody = new RequestBody();
            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);

            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(x => { throw new HttpRequestException(); });

            // act
            var actual = await apiProcessorService.PostAsync<RequestBody, List<ApiItemModel>>(url, requestBody, headerDetails);

            // assert
            Assert.Null(actual);          
            fakeLogger.Received().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
            Assert.True(httpClient.DefaultRequestHeaders.GetValues(HeaderDetails.ClientId).SequenceEqual(new string[] { "test" }));
            Assert.True(httpClient.DefaultRequestHeaders.GetValues(HeaderDetails.SessionId).SequenceEqual(new string[] { "testSesh" }));
        }

        [Fact]
        public async Task DeleteAsync_returns_good_result()
        {
            // arrange
            var expected = new List<ApiItemModel>();
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

            // act
            var actual = await apiProcessorService.DeleteAsync(url);

            // assert
            Assert.IsType<HttpStatusCode>(actual);
            Assert.Equal(HttpStatusCode.OK, actual);
        }

        [Fact]
        public async Task DeleteAsync_logs_error_and_returns_BadRequest()
        {
            // arrange
            var expected = new List<ApiItemModel>();
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadGateway
            };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

            // act
            var actual = await apiProcessorService.DeleteAsync(url);

            // assert
            Assert.IsType<HttpStatusCode>(actual);
            Assert.Equal(HttpStatusCode.BadGateway, actual);
            fakeLogger.Received().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
        }

        [Fact]
        public async Task DeleteAsync_doesnt_log_404()
        {
            // arrange
            var expected = new List<ApiItemModel>();
            var url = new Uri("https://www.somewhere.com", UriKind.Absolute);

            var fakeHttpRequestSender = Substitute.For<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };
            var fakeLogger = Substitute.For<FakeLogger<ApiService>>();

            var apiProcessorService = new ApiService(httpClient, fakeLogger);

            fakeHttpRequestSender.Send(Arg.Any<HttpRequestMessage>()).Returns(httpResponse);

            // act
            var actual = await apiProcessorService.DeleteAsync(url);

            // assert
            Assert.IsType<HttpStatusCode>(actual);
            Assert.Equal(HttpStatusCode.NotFound, actual);
            fakeLogger.DidNotReceive().Log(LogLevel.Error, Arg.Any<string>(), Arg.Any<HttpRequestException>());
        }
    }
}
