using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Glossary;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.BlobStorage;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Tests.FakeData;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DfE.GIAP.Service.ApplicationInsightsTelemetry;
using Xunit;

namespace DfE.GIAP.Service.Tests.Download
{
    [Trait("Category", "Download Service Unit Tests")]
    public class DownloadServiceTests : IClassFixture<DownloadServiceResultsFake>
    {
        private readonly IBlobStorageService _mockBlobStorageService = Substitute.For<IBlobStorageService>();
        private readonly DownloadServiceResultsFake _downloadServiceResultsFake;

        public DownloadServiceTests(DownloadServiceResultsFake downloadServiceResultsFake)
        {
            _downloadServiceResultsFake = downloadServiceResultsFake;
        }

        [Fact]
        public async Task GetGlossaryMetaDataDownloadList_Should_Return_FilesList()
        {
            var response = _downloadServiceResultsFake.GetMetaDataDetailsList();
            _mockBlobStorageService.GetFileList("Metadata/AllUsers").ReturnsForAnyArgs(response);
            var settings = _downloadServiceResultsFake.GetAppSettings();
            var eventLogging = new Mock<IEventLogging>();
            var hostingEnvironment = new Mock<IHostEnvironment>();

            var downloadService = new DownloadService(null, Options.Create(settings), _mockBlobStorageService, eventLogging.Object, hostingEnvironment.Object);

            // Act
            var result = await downloadService.GetGlossaryMetaDataDownloadList().ConfigureAwait(false);

            // Assert
            Assert.IsType<List<MetaDataDownload>>(result);
            Assert.Equal(response.Count, result.Count());
            Assert.Equal(response.First().Name, result.First().Name);
            Assert.Equal(response.First().FileName, result.First().FileName);
            Assert.Equal(response.First().Date, result.First().Date);
        }

        [Fact]
        public async Task GetGlossaryMetaDataDownFileAsync_Should_Return_File()
        {
            var response = _downloadServiceResultsFake.GetMetaDataDetailsList();
            _mockBlobStorageService.GetFileList("Metadata/AllUsers").ReturnsForAnyArgs(response);
            var settings = _downloadServiceResultsFake.GetAppSettings();
            var ms = new MemoryStream();
            var eventLogging = new Mock<IEventLogging>();
            var hostEnvironment = new Mock<IHostEnvironment>();
            var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails() { ClientId = "123456", SessionId = "654321" };

            var downloadService = new DownloadService(null, Options.Create(settings), _mockBlobStorageService, eventLogging.Object, hostEnvironment.Object);

            // Act
            // Assert
            try
            {
                await downloadService.GetGlossaryMetaDataDownFileAsync("test.csv", ms, azureFunctionHeaderDetails).ConfigureAwait(false);
                Assert.True(true);
            }
            catch
            {
                Assert.True(false);
            }
        }

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
            var fakeBlobStorage = new Mock<IBlobStorageService>();
            var eventLogging = new Mock<IEventLogging>();
            var hostEnvironment = new Mock<IHostEnvironment>();

            var downloadService = new DownloadService(apiProcessorService, fakeAppSettings.Object, fakeBlobStorage.Object, eventLogging.Object, hostEnvironment.Object);

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
            var fakeBlobStorage = new Mock<IBlobStorageService>();
            var eventLogging = new Mock<IEventLogging>();
            var hostEnvironment = new Mock<IHostEnvironment>();
            var downloadService = new DownloadService(apiProcessorService, fakeAppSettings.Object, fakeBlobStorage.Object, eventLogging.Object, hostEnvironment.Object);

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
        public async Task GetPupilPremiumFileReturnsCorrectReturnFile()
        {
            // Arrange
            var upns = new string[] { "testupn1", "testupn2" };
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
            var urls = new AzureAppSettings() { DownloadPupilPremiumByUPNFforCSVUrl = url };
            var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
            fakeAppSettings.SetupGet(x => x.Value).Returns(urls);
            var fakeBlobStorage = new Mock<IBlobStorageService>();
            var eventLogging = new Mock<IEventLogging>();
            var hostEnvironment = new Mock<IHostEnvironment>();
            var downloadService = new DownloadService(apiProcessorService, fakeAppSettings.Object, fakeBlobStorage.Object, eventLogging.Object, hostEnvironment.Object);

            // Act
            var actual = await downloadService.GetPupilPremiumCSVFile(upns, upns, confirmationGiven, azureFunctionHeaderDetails, GIAP.Common.Enums.ReturnRoute.PupilPremium);

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

            var mockApiService = Substitute.For<IApiService>();
            mockApiService.PostAsync<DownloadRequest, IEnumerable<DownloadDataType>>(
                Arg.Any<Uri>(), Arg.Any<DownloadRequest>(), Arg.Any<AzureFunctionHeaderDetails>())
                .Returns(new List<DownloadDataType>() { DownloadDataType.EYFSP });

            var url = "http://somewhere.net";
            var urls = new AzureAppSettings() { DownloadPupilsByUPNsCSVUrl = url, DownloadOptionsCheckLimit = 500 };
            var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
            fakeAppSettings.SetupGet(x => x.Value).Returns(urls);

            var sut = new DownloadService(mockApiService, fakeAppSettings.Object, null, null, null);

            // act
            var result = await sut.CheckForNoDataAvailable(upns, upns, dataTypes, azureFunctionHeaderDetails);

            // assert
            await mockApiService.Received().PostAsync<DownloadRequest, IEnumerable<CheckDownloadDataType>>(
                Arg.Any<Uri>(),
                Arg.Is<DownloadRequest>(d => d.FileType.Equals("csv") &&
                        d.UPNs.SequenceEqual(upns) &&
                        d.DataTypes.SequenceEqual(dataTypes) &&
                        d.CheckOnly == true
                    ),
                Arg.Is<AzureFunctionHeaderDetails>(azureFunctionHeaderDetails)
                );
        }
    }
}
