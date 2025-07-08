using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Tests.FakeHttpHandlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DfE.GIAP.Service.ApplicationInsightsTelemetry;
using Xunit;

namespace DfE.GIAP.Service.Tests.Download
{
    public class UlnDownloadServiceTests
    {
        [Fact]
        public async Task DownloadCSVFileService_Returns_DownloadedFile()
        {
            var selectedPupils = new string[] { "TestUPN1", "TestUPN2" };
            var selectedDownloadOptions = new string[] { "Option1", "Option2" };
            bool isConfirmationGiven = true;

            var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails { ClientId = "validClientId", SessionId = "000000" };
            var expectedFileToBeReturned = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "TestCSV",
                FileType = "csv",
                ResponseMessage = "File download successful"
            };

            var mockHttpRequestSender = new Mock<IFakeHttpRequestSender>();
            var mockHttpMessageHandler = new FakeHttpMessageHandler(mockHttpRequestSender.Object);
            var httpClient = new HttpClient(mockHttpMessageHandler);
            var httpResponse = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(expectedFileToBeReturned)) };
            mockHttpRequestSender.Setup(x => x.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponse);
            var apiProcessorService = new ApiService(httpClient, null);

            var url = "https://www.downloadsomefile.com";
            var settings = new AzureAppSettings() { DownloadPupilsByULNsUrl = url };
            var mockAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAppSettings.SetupGet(x => x.Value).Returns(settings);
            var eventLogging = new Mock<IEventLogging>();
            var hostEnvironment = new Mock<IHostEnvironment>();

            var service = new UlnDownloadService(apiProcessorService, mockAppSettings.Object, eventLogging.Object, hostEnvironment.Object);

            // Act
            var actual = await service.GetUlnCSVFile(selectedPupils, selectedDownloadOptions, isConfirmationGiven, azureFunctionHeaderDetails, GIAP.Common.Enums.ReturnRoute.UniqueLearnerNumber);

            // Assert
            Assert.IsType<ReturnFile>(actual);
            Assert.Equal(expectedFileToBeReturned.Bytes, actual.Bytes);
            Assert.Equal(expectedFileToBeReturned.FileName, actual.FileName);
            Assert.Equal(expectedFileToBeReturned.FileType, actual.FileType);
            Assert.Equal(expectedFileToBeReturned.ResponseMessage, actual.ResponseMessage);
        }

        [Fact]
        public async Task CheckForNoDataAvailable_returns_a_list_of_unavailable_data()
        {
            // arrange
            var ulns = new string[] { "testupn1", "testupn2" };
            var dataTypes = new string[] { "SEN" };
            var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails { ClientId = "12345", SessionId = "67890" };

            var mockApiService = Substitute.For<IApiService>();
            mockApiService.PostAsync<DownloadUlnRequest, IEnumerable<DownloadUlnDataType>>(
                Arg.Any<Uri>(), Arg.Any<DownloadUlnRequest>(), Arg.Any<AzureFunctionHeaderDetails>())
                .Returns(new List<DownloadUlnDataType>() { DownloadUlnDataType.SEN });

            var url = "http://somewhere.net";
            var urls = new AzureAppSettings() { DownloadPupilsByULNsUrl = url, DownloadOptionsCheckLimit = 500 };
            var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
            fakeAppSettings.SetupGet(x => x.Value).Returns(urls);

            var sut = new UlnDownloadService(mockApiService, fakeAppSettings.Object, null, null);

            // act
            var result = await sut.CheckForNoDataAvailable(ulns, dataTypes, azureFunctionHeaderDetails);

            // assert
            await mockApiService.Received().PostAsync<DownloadUlnRequest, IEnumerable<DownloadUlnDataType>>(
                Arg.Any<Uri>(),
                Arg.Is<DownloadUlnRequest>(d => 
                        d.ULNs.SequenceEqual(ulns) &&
                        d.DataTypes.SequenceEqual(dataTypes) &&
                        d.CheckOnly == true
                    ),
                Arg.Is<AzureFunctionHeaderDetails>(azureFunctionHeaderDetails)
                );
        }
    }
}
