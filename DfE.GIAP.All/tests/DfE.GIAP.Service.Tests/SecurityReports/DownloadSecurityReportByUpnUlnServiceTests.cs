using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using DfE.GIAP.Service.Download.SecurityReport;
using Moq;
using System;
using Xunit;
using DfE.GIAP.Domain.Models.Common;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using DfE.GIAP.Service.ApiProcessor;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Tests.SecurityReports
{
    public class DownloadSecurityReportByUpnUlnServiceTests
    {
        [Fact]
        public async Task SecurityReportPupilByRecordController_DownloadSecurityReportByUpnUln_GetSecurityReportByUpn_Return_Correct_File()
        {
            // Arrange
            var upns = "testupn1";
            var azureFunctionHeaderDetails = GetAzureFunctionHeaderDetails();
            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "Test-DownloadSecurityReport-ByUPN",
                FileType = "csv"
            };

            var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
            fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

            var apiProcessorService = new ApiService(httpClient, null);

            var url = "https://www.somewhere.com";
            var urls = new AzureAppSettings() { DownloadSecurityReportByUpnUrl = url };
            var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
            fakeAppSettings.SetupGet(x => x.Value).Returns(urls);


            var downloadService = new DownloadSecurityReportByUpnUlnService(apiProcessorService, fakeAppSettings.Object);

            // Act
            var actual = await downloadService.GetSecurityReportByUpn(upns, azureFunctionHeaderDetails);

            // Assert
            Assert.IsType<ReturnFile>(actual);
            Assert.Equal(expected.Bytes, actual.Bytes);
            Assert.Equal(expected.FileName, actual.FileName);
            Assert.Equal(expected.FileType, actual.FileType);

        }

        [Fact]
        public async Task SecurityReportPupilByRecordController_DownloadSecurityReportByUpnUln_GetSecurityReportByUln_Return_Correct_File()
        {
            // Arrange
            var upns = "testuln1";
            var azureFunctionHeaderDetails = GetAzureFunctionHeaderDetails();
            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "Test-DownloadSecurityReport-ByULN",
                FileType = "csv"
            };

            var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var httpResponse = new HttpResponseMessage
            { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
            fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

            var apiProcessorService = new ApiService(httpClient, null);

            var url = "https://www.somewhere.com";
            var urls = new AzureAppSettings() { DownloadSecurityReportByUlnUrl = url };
            var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
            fakeAppSettings.SetupGet(x => x.Value).Returns(urls);

            var downloadService = new DownloadSecurityReportByUpnUlnService(apiProcessorService, fakeAppSettings.Object);

            // Act
            var actual = await downloadService.GetSecurityReportByUln(upns, azureFunctionHeaderDetails);

            // Assert
            Assert.IsType<ReturnFile>(actual);
            Assert.Equal(expected.Bytes, actual.Bytes);
            Assert.Equal(expected.FileName, actual.FileName);
            Assert.Equal(expected.FileType, actual.FileType);
        }

        private AzureFunctionHeaderDetails GetAzureFunctionHeaderDetails()
        {
            var headerDetails = new AzureFunctionHeaderDetails
            {
                ClientId = Guid.NewGuid().ToString(),
                SessionId = Guid.NewGuid().ToString()
            };

            return headerDetails;
        }
    }
}
