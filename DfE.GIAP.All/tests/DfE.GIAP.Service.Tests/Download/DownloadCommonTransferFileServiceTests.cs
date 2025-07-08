using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using DfE.GIAP.Service.ApplicationInsightsTelemetry;
using Xunit;

namespace DfE.GIAP.Service.Tests.Download
{
    public class DownloadCommonTransferFileServiceTests
    {
        [Fact]
        public async Task DownloadCommonTransferFileService_Returns_DownloadedFile()
        {
            var upns = new string[] { "TestUPN1", "TestUPN2" };
            var localAuthorityNumber = "LANumber";
            var establishmentNumber = "ESTNumber";
            bool isOrganisationEstablishment = true;

            var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails { ClientId = "validClientId", SessionId = "000000" };
            var expectedFileToBeReturned = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "CTF",
                FileType = "xml",
                ResponseMessage = "File download successful"
            };

            var mockHttpRequestSender = new Mock<IFakeHttpRequestSender>();
            var mockHttpMessageHandler = new FakeHttpMessageHandler(mockHttpRequestSender.Object);
            var httpClient = new HttpClient(mockHttpMessageHandler);
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expectedFileToBeReturned)) };
            mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

            var apiProcessorService = new ApiService(httpClient, null);

            var url = "https://www.downloadsomefile.com";
            var settings = new AzureAppSettings() { DownloadCommonTransferFileUrl = url };
            var mockAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAppSettings.SetupGet(x => x.Value).Returns(settings);
            var eventLogging = new Mock<IEventLogging>();
            var hostEnvironment = new Mock<IHostEnvironment>();

            var downloadCTFService = new DownloadCommonTransferFileService(apiProcessorService, mockAppSettings.Object, eventLogging.Object, hostEnvironment.Object);

            // Act
            var actual = await downloadCTFService.GetCommonTransferFile(upns, upns, localAuthorityNumber, establishmentNumber, isOrganisationEstablishment, azureFunctionHeaderDetails, GIAP.Common.Enums.ReturnRoute.NationalPupilDatabase);

            // Assert
            Assert.IsType<ReturnFile>(actual);
            Assert.Equal(expectedFileToBeReturned.Bytes, actual.Bytes);
            Assert.Equal(expectedFileToBeReturned.FileName, actual.FileName);
            Assert.Equal(expectedFileToBeReturned.FileType, actual.FileType);
            Assert.Equal(expectedFileToBeReturned.ResponseMessage, actual.ResponseMessage);
        }
    }
}