using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.IntegrationTests.Search.SearchByKeyWords;

/// <summary>
/// Exercises the search use case against a stubbed index. The Azure Search SDK pipeline is real -
/// only its transport is replaced - so no emulator, server or certificate is needed.
/// </summary>
public sealed class SearchByKeyWordsUseCaseIntegrationTests : BaseIntegrationTest
{
    private const string FurtherEducationIndexName = "FE_INDEX_NAME";

    private readonly AzureSearchIndexStub _searchIndexStub =
        AzureSearchIndexStub.Create()
            .WithIndexResponseFromFile(
                indexName: FurtherEducationIndexName,
                fileName: "fe_searchindex_returns_many_pupils.json");

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
            .AddStubbedSearchIndex(_searchIndexStub)
            .AddSearchCore(searchConfiguration);

        return Task.CompletedTask;
    }

    [Fact]
    public async Task SearchByKeyWordsUseCase_Returns_Results_When_HandleRequest()
    {
        // Arrange
        IUseCase<FurtherEducationSearchByNameRequest, SearchResponse<FurtherEducationLearners>> sut =
            ResolveApplicationType<IUseCase<FurtherEducationSearchByNameRequest, SearchResponse<FurtherEducationLearners>>>()!;

        SortOrder sortOrder = new(
            sortField: "Forename",
            sortDirection: "desc",
            validSortFields: ["Forename", "Surname"]);

        SearchCriteria searchCriteria = new()
        {
            Index = FurtherEducationIndexName,
            SearchFields = ["field1"],
            Size = 20
        };

        FurtherEducationSearchByNameRequest request = new()
        {
            SearchKeywords = "test",
            SearchCriteria = searchCriteria,
            SortOrder = sortOrder
        };

        // Act
        SearchResponse<FurtherEducationLearners> response = await sut.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.LearnerSearchResults);
        Assert.Equal(10, response.TotalNumberOfResults);

        RecordedSearchRequest searchRequest = Assert.Single(_searchIndexStub.ReceivedRequests);
        Assert.Equal(FurtherEducationIndexName, searchRequest.IndexName);
    }
}
