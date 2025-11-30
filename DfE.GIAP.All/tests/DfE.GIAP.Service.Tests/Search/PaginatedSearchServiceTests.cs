using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.Search;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Service.Tests.Search;

public class PaginatedSearchServiceTests
{
    [Fact]
    public async Task GetPage_works_correctly()
    {
        // arrange
        var headerDetails = new AzureFunctionHeaderDetails()
        {
            ClientId = "test",
            SessionId = "testSesh"
        };

        var options = Substitute.For<IOptions<AzureAppSettings>>();
        options.Value.Returns(new AzureAppSettings()
        {
            PaginatedSearchUrl = "http://test.com/api/get-page/{indexType}/{queryType}?code=testcode"
        });

        var expectedPaginatedSearchUrl = "http://test.com/api/get-page/NPD/Numbers?code=testcode";
        var expectedUri = new Uri(expectedPaginatedSearchUrl);

        var httpClient = new HttpClient();
        var mockApiService = Substitute.For<IApiService>();
        var eventLogger = new Mock<IEventLogger>();

        mockApiService.PostAsync<PaginatedSearchRequest, PaginatedResponse>
            (Arg.Any<Uri>(), Arg.Any<PaginatedSearchRequest>(), Arg.Any<AzureFunctionHeaderDetails>()).Returns(new PaginatedResponse());

        var paginatedService = new PaginatedSearchService(
            mockApiService,
            options,
            eventLogger.Object);

        // act
        var response = await paginatedService.GetPage(
            "test",
            null,
            20,
            0,
            AzureSearchIndexType.NPD,
            AzureSearchQueryType.Numbers,
            headerDetails,
            "",
            "");

        // assert
        Assert.IsType<PaginatedResponse>(response);
        await mockApiService.Received().PostAsync<PaginatedSearchRequest, PaginatedResponse>(
            Arg.Is<Uri>(u => u.Equals(expectedUri)),
            Arg.Is<PaginatedSearchRequest>(r =>
                r.SearchText.Equals("test") &&
                r.Filters == null &&
                r.PageNumber == 0 &&
                r.PageSize == 20),
            Arg.Is<AzureFunctionHeaderDetails>(headerDetails)
            );
    }
}
