using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.Content;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DfE.GIAP.Service.Tests.Content;

[Trait("Category", "Common Giap Service Unit Tests")]
public class ContentServiceTests
{
    private readonly Mock<IApiService> _mockApiService = new Mock<IApiService>();
    private readonly Mock<IOptions<AzureAppSettings>> _mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();

    [Theory]
    [InlineData(DocumentType.Landing)]
    [InlineData(DocumentType.PrivacyNotice)]
    public async Task ContentService_GetContent_Returns_Data_Successfully(DocumentType documentType)
    {
        var expectedResponse = new CommonResponseBody
        {
            Title = "Title",
            Body = "Body",
            Id = documentType.ToString()
        };
        _mockApiService.Setup(x => x.GetAsync<CommonResponseBody>(It.IsAny<Uri>())).ReturnsAsync(expectedResponse);

        var url = "https://www.somewhere.com?code=testcode";
        var urls = new AzureAppSettings() { GetContentByIDUrl = url };
        _mockAzureAppSettings.SetupGet(x => x.Value).Returns(urls);

        var contentService = new ContentService(_mockApiService.Object, _mockAzureAppSettings.Object);

        // act
        var actual = await contentService.GetContent(documentType).ConfigureAwait(false);

        // assert
        Assert.IsType<CommonResponseBody>(actual);
        Assert.Equal(expectedResponse.Id, actual.Id);
    }

    [Theory]
    [InlineData(ActionTypes.Publish)]
    [InlineData(ActionTypes.Unpublish)]
    public async Task ContentService_SetDocumentToPublished_Returns_Data_Successfully(ActionTypes actionType)
    {
        var requestBody = new CommonRequestBody
        {
            Title = "Title",
            Body = "Body",
            Action = (int)actionType
        };
        var expectedResponse = new CommonResponseBody
        {
            Title = "Title",
            Body = "Body",
            Published = actionType == ActionTypes.Publish
        };
        var headerDetails = new AzureFunctionHeaderDetails()
        {
            ClientId = "test",
            SessionId = "testSesh"
        };
        _mockApiService.Setup(x => x.PostAsync<CommonRequestBody, CommonResponseBody>(It.IsAny<Uri>(), It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(expectedResponse);

        var url = "https://www.somewhere.com?code=testcode";
        var urls = new AzureAppSettings() { UpdateNewsPropertyUrl = url };
        _mockAzureAppSettings.SetupGet(x => x.Value).Returns(urls);

        var contentService = new ContentService(_mockApiService.Object, _mockAzureAppSettings.Object);

        // act
        var actual = await contentService.SetDocumentToPublished(requestBody, headerDetails).ConfigureAwait(false);

        // assert
        Assert.IsType<CommonResponseBody>(actual);
        Assert.Equal(expectedResponse.Title, actual.Title);
        Assert.Equal(expectedResponse.Body, actual.Body);
        Assert.Equal(expectedResponse.Published, actual.Published);
    }

    [Fact]
    public async Task ContentService_AddOrUpdateDocument_Returns_Data_Successfully()
    {
        var requestBody = new CommonRequestBody
        {
            Title = "Title",
            Body = "Body"
        };
        var expectedResponse = new CommonResponseBody
        {
            Title = "Title",
            Body = "Body",
            Published = false
        };
        var headerDetails = new AzureFunctionHeaderDetails()
        {
            ClientId = "test",
            SessionId = "testSesh"
        };
        _mockApiService.Setup(x => x.PostAsync<CommonRequestBody, CommonResponseBody>(It.IsAny<Uri>(), It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(expectedResponse);

        var url = "https://www.somewhere.com?code=testcode";
        var urls = new AzureAppSettings() { UpdateNewsDocumentUrl = url };
        _mockAzureAppSettings.SetupGet(x => x.Value).Returns(urls);

        var contentService = new ContentService(_mockApiService.Object, _mockAzureAppSettings.Object);

        // act
        var actual = await contentService.AddOrUpdateDocument(requestBody, headerDetails).ConfigureAwait(false);

        // assert
        Assert.IsType<CommonResponseBody>(actual);
        Assert.Equal(expectedResponse.Title, actual.Title);
        Assert.Equal(expectedResponse.Body, actual.Body);
    }
}
