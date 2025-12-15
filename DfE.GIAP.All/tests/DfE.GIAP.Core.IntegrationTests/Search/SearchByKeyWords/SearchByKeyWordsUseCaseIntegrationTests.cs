using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Request;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.SharedTests.Infrastructure.WireMock;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Request;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Response;
using DfE.GIAP.SharedTests.TestDoubles.Configuration;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.IntegrationTests.Search.SearchByKeyWords;

public class SearchByKeyWordsUseCaseIntegrationTests : BaseIntegrationTest
{
    private readonly WireMockServerFixture _searchIndexFixture;

    public SearchByKeyWordsUseCaseIntegrationTests(WireMockServerFixture searchIndexFixture)
    {
        ArgumentNullException.ThrowIfNull(searchIndexFixture);
        _searchIndexFixture = searchIndexFixture;

    }

    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        IConfiguration searchConfiguration =
            ConfigurationTestDoubles.DefaultConfigurationBuilder()
                .WithAzureSearchOptions()
                .WithAzureSearchConnectionOptions()
                .WithSearchCriteriaOptions()
                .WithFilterKeyToFilterExpressionMapOptions()
                .Build();

        services
            .AddSearchDependencies(searchConfiguration);

        return Task.CompletedTask;
    }

    [Fact]
    public async Task SearchByKeyWordsUseCase_Returns_Results_When_HandleRequest()
    {
        HttpMappingRequest httpRequest = HttpMappingRequest.Create(
            httpMappingFiles: [
                new HttpMappingFile(
                    key: "index-names",
                    fileName: "get_searchindex_names.json"),
                new HttpMappingFile(
                    key: "further-education",
                    fileName: "fe_searchindex_returns_many_pupils.json")
            ]);

        HttpMappedResponses stubbedResponses = await _searchIndexFixture.RegisterHttpMapping(httpRequest);

        IUseCase<SearchRequest, SearchResponse> sut =
            ResolveApplicationType<IUseCase<SearchRequest, SearchResponse>>()!;

        SortOrder sortOrder = new(
            sortField: "Forename",
            sortDirection: "desc",
            validSortFields: ["Forename", "Surname"]);

        SearchRequest request = new(searchIndexKey: "further-education", searchKeywords: "test", sortOrder);

        // act
        SearchResponse response = await sut.HandleRequestAsync(request);

        // assert
        Assert.NotNull(response);
        Assert.NotNull(response.LearnerSearchResults);
        Assert.Equal(SearchResponseStatus.Success, response.Status);
        Assert.Equal(10, response.TotalNumberOfResults);
    }
}
