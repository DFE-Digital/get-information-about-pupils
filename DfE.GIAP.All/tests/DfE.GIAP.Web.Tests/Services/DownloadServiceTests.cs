using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Web.Services.ApiProcessor;
using DfE.GIAP.Web.Services.Download;
using DfE.GIAP.Web.Tests.Services.FakeHttpHandlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Tests.Services;

[Trait("Category", "Download Service Unit Tests")]
public sealed class DownloadServiceTests
{
    [Fact]
    public async Task GetCSVFileReturnsCorrectReturnFile()
    {
        // Arrange
        string[] upns = ["testupn1", "testupn2"];
        string[] dataTypes = ["KS1", "KS2"];
        bool confirmationGiven = true;
        AzureFunctionHeaderDetails azureFunctionHeaderDetails = new() { ClientId = "12345", SessionId = "67890" };
        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "Test-CSV-file",
            FileType = "csv",
            RemovedUpns = ["removedupn1", "removedupn2"],
            ResponseMessage = "Test response message"
        };

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new() { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        ApiService apiProcessorService = new(httpClient, null);

        string url = "https://www.somewhere.com";
        AzureAppSettings urls = new() { DownloadPupilsByUPNsCSVUrl = url };
        Mock<IOptions<AzureAppSettings>> fakeAppSettings = new();
        fakeAppSettings.SetupGet(x => x.Value).Returns(urls);
        Mock<IEventLogger> eventLogging = new();

        DownloadService downloadService = new(fakeAppSettings.Object, apiProcessorService, eventLogging.Object);

        // Act
        ReturnFile actual = await downloadService.GetCSVFile(upns, upns, dataTypes, confirmationGiven, azureFunctionHeaderDetails, ReturnRoute.NationalPupilDatabase);

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
        string[] upns = ["testupn1", "testupn2"];
        string[] dataTypes = ["KS1", "KS2"];
        bool confirmationGiven = true;
        AzureFunctionHeaderDetails azureFunctionHeaderDetails = new() { ClientId = "12345", SessionId = "67890" };
        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "Test-TAB-file",
            FileType = "tab",
            RemovedUpns = ["removedupn1", "removedupn2"],
            ResponseMessage = "Test response message"
        };

        Mock<IFakeHttpRequestSender> fakeHttpRequestSender = new();
        FakeHttpMessageHandler fakeHttpMessageHandler = new(fakeHttpRequestSender.Object);
        HttpClient httpClient = new(fakeHttpMessageHandler);
        HttpResponseMessage httpResponse = new() { Content = new StringContent(JsonConvert.SerializeObject(expected)) };
        fakeHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);

        ApiService apiProcessorService = new(httpClient, null);

        string url = "https://www.somewhere.com";
        AzureAppSettings urls = new() { DownloadPupilsByUPNsCSVUrl = url };
        Mock<IOptions<AzureAppSettings>> fakeAppSettings = new();
        fakeAppSettings.SetupGet(x => x.Value).Returns(urls);
        Mock<IEventLogger> eventLogging = new();
        Mock<IHostEnvironment> hostEnvironment = new();
        DownloadService downloadService = new(fakeAppSettings.Object, apiProcessorService, eventLogging.Object);

        // Act
        ReturnFile actual = await downloadService.GetTABFile(upns, upns, dataTypes, confirmationGiven, azureFunctionHeaderDetails, ReturnRoute.NationalPupilDatabase);

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
        string[] upns = ["testupn1", "testupn2"];
        string[] dataTypes = ["KS1", "KS2"];
        AzureFunctionHeaderDetails azureFunctionHeaderDetails = new() { ClientId = "12345", SessionId = "67890" };


        Mock<IApiService> mockApiService = new();
        mockApiService.Setup(t =>
            t.PostAsync<DownloadRequest, IEnumerable<DownloadDataType>>(
                It.IsAny<Uri>(),
                It.IsAny<DownloadRequest>(),
                It.IsAny<AzureFunctionHeaderDetails>()))
            .ReturnsAsync(new List<DownloadDataType>() { DownloadDataType.EYFSP });

        string url = "http://somewhere.net";
        AzureAppSettings urls = new() { DownloadPupilsByUPNsCSVUrl = url, DownloadOptionsCheckLimit = 500 };
        Mock<IOptions<AzureAppSettings>> fakeAppSettings = new();
        Mock<IEventLogger> eventLogging = new();
        fakeAppSettings.SetupGet(x => x.Value).Returns(urls);

        DownloadService sut = new(fakeAppSettings.Object, mockApiService.Object, eventLogging.Object);

        // act
        IEnumerable<CheckDownloadDataType> result = await sut.CheckForNoDataAvailable(upns, upns, dataTypes, azureFunctionHeaderDetails);

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
