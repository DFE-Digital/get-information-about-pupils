using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.Download.SecurityReport;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace DfE.GIAP.Service.Tests.Services.SecurityReports
{
    public class DownloadSecurityReportDetailedSearchesServiceTests
    {
        private readonly Mock<IApiService> _apiProcessorService;

        public DownloadSecurityReportDetailedSearchesServiceTests()
        {
            _apiProcessorService = new Mock<IApiService>();
        }

        [Fact]
        public async Task GetSecurityReportDetailedSearches_Return_Correct_File()
        {
            // Arrange
            var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails {ClientId = "12345", SessionId = "67890"};
            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "Test-DownloadSecurityReport",
                FileType = "csv"
            };

            var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
                {Content = new StringContent(JsonConvert.SerializeObject(expected))};
            fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

            var apiProcessorService = new ApiService(httpClient, null);

            var url = "https://www.somewhere.com";
            var urls = new AzureAppSettings() {DownloadSecurityReportDetailedSearchesUrl = url};
            var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
            fakeAppSettings.SetupGet(x => x.Value).Returns(urls);
            var downloadService =
                new DownloadSecurityReportDetailedSearchesService(apiProcessorService, fakeAppSettings.Object);

            // Act
            var actual = await downloadService.GetSecurityReportDetailedSearches("123456", SecurityReportSearchType.UniqueReferenceNumber,
                azureFunctionHeaderDetails, false);

            // Assert
            Assert.IsType<ReturnFile>(actual);
            Assert.Equal(expected.Bytes, actual.Bytes);
            Assert.Equal(expected.FileName, actual.FileName);
            Assert.Equal(expected.FileType, actual.FileType);
        }
    }
    }
