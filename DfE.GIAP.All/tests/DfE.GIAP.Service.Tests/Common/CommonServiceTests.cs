using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.LoggingEvent;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Service.Tests.Common
{
    public class CommonServiceTests
    {
        private readonly HttpClient _mockHttpClient;
        private IApiService _mockApiProcessorService;
        private readonly Mock<IOptions<AzureAppSettings>> _mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
        private readonly Mock<IFakeHttpRequestSender> _mockHttpRequestSender;
        private readonly FakeHttpMessageHandler _mockHttpMessageHandler;

        public CommonServiceTests()
        {
            _mockHttpRequestSender = new Mock<IFakeHttpRequestSender>();
            _mockHttpMessageHandler = new FakeHttpMessageHandler(_mockHttpRequestSender.Object);
            _mockHttpClient = new HttpClient(_mockHttpMessageHandler);
        }

        [Fact]
        public async Task CommonService_GetLatestNewsStatus_Returns_True_When_LatestNewsFound()
        {
            //Arrange
            var expectedResponse = "true";
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)) };
            _mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

            _mockApiProcessorService = new ApiService(_mockHttpClient, null);
            var url = "https://www.mockwebsite.com";
            _mockAzureAppSettings.SetupGet(x => x.Value).Returns(new AzureAppSettings() { GetLatestNewsStatusUrl = url });
            var service = new CommonService(_mockApiProcessorService, _mockAzureAppSettings.Object);

            //Act
            var actual = await service.GetLatestNewsStatus(It.IsAny<string>());

            //Assert
            Assert.True(actual);
        }

        [Fact]
        public async Task CommonService_GetLatestNewsStatus_Returns_False_When_LatestNews_NotFound()
        {
            //Arrange
            var expectedResponse = "false";
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)) };
            _mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

            _mockApiProcessorService = new ApiService(_mockHttpClient, null);
            var url = "https://www.mockwebsite.com";
            _mockAzureAppSettings.SetupGet(x => x.Value).Returns(new AzureAppSettings() { GetLatestNewsStatusUrl = url });
            var service = new CommonService(_mockApiProcessorService, _mockAzureAppSettings.Object);

            //Act
            var actual = await service.GetLatestNewsStatus(It.IsAny<string>());

            //Assert
            Assert.False(actual);
        }

        [Fact]
        public async Task CommonService_GetLatestNewsStatus_Returns_False_When_LatestNews_Throws_Exception()
        {
            //Arrange

            _mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Throws(It.IsAny<Exception>());
            _mockApiProcessorService = new ApiService(_mockHttpClient, null);

            var url = "https://www.mockwebsite.com";
            _mockAzureAppSettings.SetupGet(x => x.Value).Returns(new AzureAppSettings() { GetLatestNewsStatusUrl = url });
            var service = new CommonService(_mockApiProcessorService, _mockAzureAppSettings.Object);

            //Act
            var actual = await service.GetLatestNewsStatus(It.IsAny<string>());

            //Assert
            Assert.False(actual);
        }

        [Fact]
        public async Task CommonService_CreateLoggingEvent_Returns_True_When_EventLogged()
        {
            //Arrange
            var expectedResponse = "true";
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)) };
            _mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

            _mockApiProcessorService = new ApiService(_mockHttpClient, null);
            var url = "https://www.mockwebsite.com";
            _mockAzureAppSettings.SetupGet(x => x.Value).Returns(new AzureAppSettings() { LoggingEventUrl = url });
            var service = new CommonService(_mockApiProcessorService, _mockAzureAppSettings.Object);

            //Act
            var actual = await service.CreateLoggingEvent(It.IsAny<LoggingEvent>());

            //Assert
            Assert.True(actual);
        }

        [Fact]
        public async Task CommonService_CreateLoggingEvent_Returns_False_When_Event_NotLogged()
        {
            //Arrange
            var expectedResponse = "false";
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)) };
            _mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

            _mockApiProcessorService = new ApiService(_mockHttpClient, null);
            var url = "https://www.mockwebsite.com";
            _mockAzureAppSettings.SetupGet(x => x.Value).Returns(new AzureAppSettings() { LoggingEventUrl = url });
            var service = new CommonService(_mockApiProcessorService, _mockAzureAppSettings.Object);

            //Act
            var actual = await service.CreateLoggingEvent(It.IsAny<LoggingEvent>());

            //Assert
            Assert.False(actual);
        }

        [Fact]
        public async Task CommonService_CreateLoggingEvent_Returns_False_When_Exception_Is_Thrown()
        {
            //Arrange
            _mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Throws(It.IsAny<Exception>());
            _mockApiProcessorService = new ApiService(_mockHttpClient, null);

            var url = "https://www.mockwebsite.com";
            _mockAzureAppSettings.SetupGet(x => x.Value).Returns(new AzureAppSettings() { LoggingEventUrl = url });
            var service = new CommonService(_mockApiProcessorService, _mockAzureAppSettings.Object);

            //Act
            var actual = await service.CreateLoggingEvent(It.IsAny<LoggingEvent>());

            //Assert
            Assert.False(actual);
        }

        [Fact]
        public async Task CommonService_GetUserProfile_Returns_UserProfile_When_Found()
        {
            //Arrange
            var expectedResponse = new UserProfile()
            {
                IsPupilListUpdated = true,
                PupilList = new string[] { "Pupil1", "Pupil2" },
                UserId = "validId"
            };
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)) };
            _mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

            _mockApiProcessorService = new ApiService(_mockHttpClient, null);
            var url = "https://www.mockwebsite.com";
            _mockAzureAppSettings.SetupGet(x => x.Value).Returns(new AzureAppSettings() { GetUserProfileUrl = url });
            var service = new CommonService(_mockApiProcessorService, _mockAzureAppSettings.Object);

            //Act
            var actual = await service.GetUserProfile(It.IsAny<UserProfile>());

            //Assert
            Assert.IsType<UserProfile>(actual);
            Assert.Equal(expectedResponse.UserId, actual.UserId);
            Assert.Equal(expectedResponse.IsPupilListUpdated, actual.IsPupilListUpdated);
            Assert.Equal(expectedResponse.PupilList.Length, actual.PupilList.Length);
        }

        [Fact]
        public async Task CommonService_GetUserProfile_Returns_Null_When_NotFound()
        {
            //Arrange
            var httpResponse = new HttpResponseMessage { Content = null };
            _mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

            _mockApiProcessorService = new ApiService(_mockHttpClient, null);
            var url = "https://www.mockwebsite.com";
            _mockAzureAppSettings.SetupGet(x => x.Value).Returns(new AzureAppSettings() { GetUserProfileUrl = url });
            var service = new CommonService(_mockApiProcessorService, _mockAzureAppSettings.Object);

            //Act
            var actual = await service.GetUserProfile(It.IsAny<UserProfile>());

            //Assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task CommonService_CreateOrUpdateUserProfile_Returns_True_When_UserProfileCreatedOrUpdated()
        {
            //Arrange
            var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails { ClientId = "validClientId", SessionId = "000000" };
            var expectedResponse = "true";
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)) };
            _mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);
            var apiProcessorService = new ApiService(_mockHttpClient, null);

            var url = "https://www.downloadsomefile.com";
            var settings = new AzureAppSettings() { CreateOrUpdateUserProfileUrl = url };
            _mockAzureAppSettings.SetupGet(x => x.Value).Returns(settings);

            var service = new CommonService(apiProcessorService, _mockAzureAppSettings.Object);

            // Act
            var actual = await service.CreateOrUpdateUserProfile(It.IsAny<UserProfile>(), azureFunctionHeaderDetails);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public async Task CommonService_CreateOrUpdateUserProfile_Returns_False_When_Exception_Is_Thrown()
        {
            //Arrange
            var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails { ClientId = "validClientId", SessionId = "000000" };
            _mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Throws(It.IsAny<Exception>());
            var apiProcessorService = new ApiService(_mockHttpClient, null);

            var url = "https://www.downloadsomefile.com";
            var settings = new AzureAppSettings() { CreateOrUpdateUserProfileUrl = url };
            _mockAzureAppSettings.SetupGet(x => x.Value).Returns(settings);

            var service = new CommonService(apiProcessorService, _mockAzureAppSettings.Object);

            // Act
            var actual = await service.CreateOrUpdateUserProfile(It.IsAny<UserProfile>(), azureFunctionHeaderDetails);

            // Assert
            Assert.False(actual);
        }
    }
}
