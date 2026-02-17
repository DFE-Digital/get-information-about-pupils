using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.IntegrationTests.Search.SearchByKeyWords;

public sealed class SearchByKeyWordsUseCaseIntegrationTests : BaseIntegrationTest
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
                .WithSearchOptions()
                .WithSearchIndexNameOptions()
                .WithAzureSearchConnectionOptions()
                .WithFilterKeyToFilterExpressionMapOptions()
                .Build();

        services
            .AddSearchCore(searchConfiguration);
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

        IUseCase<FurtherEducationSearchByNameRequest, SearchResponse<FurtherEducationLearners>> sut =
            ResolveApplicationType<IUseCase<FurtherEducationSearchByNameRequest, SearchResponse<FurtherEducationLearners>>>()!;

        SortOrder sortOrder = new(
            sortField: "Forename",
            sortDirection: "desc",
            validSortFields: ["Forename", "Surname"]);

        SearchCriteria searchCriteria = new()
        {
            Index = "FE_INDEX_NAME",
            SearchFields = ["field1"],
            Size = 20
        };

        FurtherEducationSearchByNameRequest request = new()
        {
            SearchKeywords = "test",
            SearchCriteria = searchCriteria,
            SortOrder = sortOrder
        };

        // act
        SearchResponse<FurtherEducationLearners> response = await sut.HandleRequestAsync(request);

        // assert
        Assert.NotNull(response);
        Assert.NotNull(response.LearnerSearchResults);
        Assert.Equal(10, response.TotalNumberOfResults);
    }
}
