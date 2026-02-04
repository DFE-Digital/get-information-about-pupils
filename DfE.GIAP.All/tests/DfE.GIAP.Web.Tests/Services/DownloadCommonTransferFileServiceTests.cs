using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using DfE.GIAP.Web.Services.ApiProcessor;
using DfE.GIAP.Web.Services.Download.CTF;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DfE.GIAP.Service.Tests.Download;

public class DownloadCommonTransferFileServiceTests
{
    [Fact]
    public async Task DownloadCommonTransferFileService_Returns_DownloadedFile()
    {
        string[] upns = new string[] { "TestUPN1", "TestUPN2" };
        string localAuthorityNumber = "LANumber";
        string establishmentNumber = "ESTNumber";
        bool isOrganisationEstablishment = true;

        AzureFunctionHeaderDetails azureFunctionHeaderDetails = new AzureFunctionHeaderDetails { ClientId = "validClientId", SessionId = "000000" };
        ReturnFile expectedFileToBeReturned = new ReturnFile()
        {
            Bytes = new byte[200],
            FileName = "CTF",
            FileType = "xml",
            ResponseMessage = "File download successful"
        };

        Mock<IFakeHttpRequestSender> mockHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        using FakeHttpMessageHandler mockHttpMessageHandler = new FakeHttpMessageHandler(mockHttpRequestSender.Object);
        using HttpClient httpClient = new HttpClient(mockHttpMessageHandler);
        using HttpResponseMessage httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expectedFileToBeReturned)) };
        mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        ApiService apiProcessorService = new ApiService(httpClient, null);

        string url = "https://www.downloadsomefile.com";
        AzureAppSettings settings = new AzureAppSettings() { DownloadCommonTransferFileUrl = url };
        Mock<IOptions<AzureAppSettings>> mockAppSettings = new Mock<IOptions<AzureAppSettings>>();
        mockAppSettings.SetupGet(x => x.Value).Returns(settings);
        Mock<IEventLogger> eventLogger = new Mock<IEventLogger>();

        DownloadCommonTransferFileService downloadCTFService = new DownloadCommonTransferFileService(apiProcessorService, mockAppSettings.Object, eventLogger.Object);

        // Act
        ReturnFile actual = await downloadCTFService.GetCommonTransferFile(upns, upns, localAuthorityNumber, establishmentNumber, isOrganisationEstablishment, azureFunctionHeaderDetails, GIAP.Common.Enums.ReturnRoute.NationalPupilDatabase);

        // Assert
        Assert.IsType<ReturnFile>(actual);
        Assert.Equal(expectedFileToBeReturned.Bytes, actual.Bytes);
        Assert.Equal(expectedFileToBeReturned.FileName, actual.FileName);
        Assert.Equal(expectedFileToBeReturned.FileType, actual.FileType);
        Assert.Equal(expectedFileToBeReturned.ResponseMessage, actual.ResponseMessage);
    }
}
