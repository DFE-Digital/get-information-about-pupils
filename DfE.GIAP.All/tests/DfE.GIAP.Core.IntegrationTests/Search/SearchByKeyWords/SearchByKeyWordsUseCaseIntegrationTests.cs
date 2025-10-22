
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Request;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.SharedTests.Infrastructure.SearchIndex;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Configuration;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.IntegrationTests.Search.SearchByKeyWords;

public class SearchByKeyWordsUseCaseIntegrationTests : BaseIntegrationTest
{
    private SearchIndexFixture _mockSearchFixture = null!;

    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        _mockSearchFixture = new();

        IConfiguration searchConfiguration =
            ConfigurationTestDoubles.DefaultConfigurationBuilder()
                .WithSearchIndexOptions()
                .WithAzureSearchOptions()
                .WithAzureSearchConnectionOptions()
                .WithSearchCriteriaOptions()
                .Build();

        services
            .AddSearchDependencies(searchConfiguration);

        return Task.CompletedTask;
    }

    [Fact]
    public async Task SearchByKeyWordsUseCase_Returns_Results_When_HandleRequest()
    {
        await _mockSearchFixture.StubAvailableIndexes(["FE_INDEX_NAME"]);

        List<AzureIndexEntity> furtherEducationSearchIndexDtos =
            await _mockSearchFixture.StubFurtherEducationIndex(values: AzureIndexEntityDtosTestDoubles.Generate(count: 30));

        IUseCase<SearchRequest, SearchResponse> sut =
            ResolveTypeFromScopedContext<IUseCase<SearchRequest, SearchResponse>>()!;

        SortOrder sortOrder = new(
            sortField: "Forename",
            sortDirection: "desc",
            validSortFields: ["Forename", "Surname"]);

        SearchRequest request = new(searchKeywords: "test", sortOrder);

        // act
        SearchResponse response = await sut.HandleRequestAsync(request);

        // assert
        Assert.NotNull(response);
        Assert.NotNull(response.LearnerSearchResults);
        Assert.Equal(SearchResponseStatus.Success, response.Status);
        Assert.Equal(30, response.TotalNumberOfResults);
    }

    protected override Task OnDisposeAsync()
    {
        _mockSearchFixture?.Dispose();
        return Task.CompletedTask;
    }
}
