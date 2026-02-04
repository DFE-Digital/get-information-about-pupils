using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using DfE.GIAP.Web.Services.ApiProcessor;
using DfE.GIAP.Web.Services.Download;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DfE.GIAP.Service.Tests.Download;

[Trait("Category", "Download Service Unit Tests")]
public sealed class DownloadServiceTests
{
    [Fact]
    public async Task GetCSVFileReturnsCorrectReturnFile()
    {
        // Arrange
        var upns = new string[] { "testupn1", "testupn2" };
        var dataTypes = new string[] { "KS1", "KS2" };
        bool confirmationGiven = true;
        var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails { ClientId = "12345", SessionId = "67890" };
        var expected = new ReturnFile()
        {
            Bytes = new byte[200],
            FileName = "Test-CSV-file",
            FileType = "csv",
            RemovedUpns = new string[] { "removedupn1", "removedupn2" },
            ResponseMessage = "Test response message"
        };

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        var apiProcessorService = new ApiService(httpClient, null);

        var url = "https://www.somewhere.com";
        var urls = new AzureAppSettings() { DownloadPupilsByUPNsCSVUrl = url };
        var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
        fakeAppSettings.SetupGet(x => x.Value).Returns(urls);
        var eventLogging = new Mock<IEventLogger>();

        var downloadService = new DownloadService(fakeAppSettings.Object, apiProcessorService, eventLogging.Object);

        // Act
        var actual = await downloadService.GetCSVFile(upns, upns, dataTypes, confirmationGiven, azureFunctionHeaderDetails, GIAP.Common.Enums.ReturnRoute.NationalPupilDatabase);

        // Assert
        Assert.IsType<ReturnFile>(actual);
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.FileName, actual.FileName);
        Assert.Equal(expected.FileType, actual.FileType);
        Assert.Equal(expected.RemovedUpns, actual.RemovedUpns);
        Assert.Equal(expected.ResponseMessage, actual.ResponseMessage);
    }

    [Fact]
    public async Task GetTABFileReturnsCorrectReturnFile()
    {
        // Arrange
        var upns = new string[] { "testupn1", "testupn2" };
        var dataTypes = new string[] { "KS1", "KS2" };
        bool confirmationGiven = true;
        var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails { ClientId = "12345", SessionId = "67890" };
        var expected = new ReturnFile()
        {
            Bytes = new byte[200],
            FileName = "Test-TAB-file",
            FileType = "tab",
            RemovedUpns = new string[] { "removedupn1", "removedupn2" },
            ResponseMessage = "Test response message"
        };

        var fakeHttpRequestSender = new Mock<IFakeHttpRequestSender>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender.Object);
        var httpClient = new HttpClient(fakeHttpMessageHandler);
        var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        var apiProcessorService = new ApiService(httpClient, null);

        var url = "https://www.somewhere.com";
        var urls = new AzureAppSettings() { DownloadPupilsByUPNsCSVUrl = url };
        var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
        fakeAppSettings.SetupGet(x => x.Value).Returns(urls);
        var eventLogging = new Mock<IEventLogger>();
        var hostEnvironment = new Mock<IHostEnvironment>();
        var downloadService = new DownloadService(fakeAppSettings.Object, apiProcessorService, eventLogging.Object);

        // Act
        var actual = await downloadService.GetTABFile(upns, upns, dataTypes, confirmationGiven, azureFunctionHeaderDetails, GIAP.Common.Enums.ReturnRoute.NationalPupilDatabase);

        // Assert
        Assert.IsType<ReturnFile>(actual);
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.FileName, actual.FileName);
        Assert.Equal(expected.FileType, actual.FileType);
        Assert.Equal(expected.RemovedUpns, actual.RemovedUpns);
        Assert.Equal(expected.ResponseMessage, actual.ResponseMessage);
    }

    [Fact]
    public async Task CheckForNoDataAvailable_returns_a_list_of_unavailable_data()
    {
        // arrange
        var upns = new string[] { "testupn1", "testupn2" };
        var dataTypes = new string[] { "KS1", "KS2" };
        var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails { ClientId = "12345", SessionId = "67890" };

        
        var mockApiService = new Mock<IApiService>();
        mockApiService.Setup(t =>
            t.PostAsync<DownloadRequest, IEnumerable<DownloadDataType>>(
                It.IsAny<Uri>(),
                It.IsAny<DownloadRequest>(),
                It.IsAny<AzureFunctionHeaderDetails>()))
            .ReturnsAsync(new List<DownloadDataType>() { DownloadDataType.EYFSP });

        var url = "http://somewhere.net";
        var urls = new AzureAppSettings() { DownloadPupilsByUPNsCSVUrl = url, DownloadOptionsCheckLimit = 500 };
        var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
        var eventLogging = new Mock<IEventLogger>();
        fakeAppSettings.SetupGet(x => x.Value).Returns(urls);

        var sut = new DownloadService(fakeAppSettings.Object, mockApiService.Object, eventLogging.Object);

        // act
        var result = await sut.CheckForNoDataAvailable(upns, upns, dataTypes, azureFunctionHeaderDetails);

        // assert
        mockApiService.Verify(t =>
            t.PostAsync<DownloadRequest, IEnumerable<CheckDownloadDataType>>(
                It.IsAny<Uri>(),
                It.Is<DownloadRequest>(d => d.FileType.Equals("csv") &&
                    d.UPNs.SequenceEqual(upns) &&
                    d.DataTypes.SequenceEqual(dataTypes) &&
                    d.CheckOnly),
                It.IsAny<AzureFunctionHeaderDetails>()), Times.Once);
    }
}
