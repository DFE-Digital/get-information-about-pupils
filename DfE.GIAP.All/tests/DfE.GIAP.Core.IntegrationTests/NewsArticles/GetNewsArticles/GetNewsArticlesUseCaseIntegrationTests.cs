using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.NewsArticles.Application.Enums;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.GetNewsArticles;

public sealed class GetNewsArticlesUseCaseIntegrationTests : BaseIntegrationTest
{
    private readonly CosmosDbFixture _cosmosDbFixture;

    public GetNewsArticlesUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture)
    {
        ArgumentNullException.ThrowIfNull(cosmosDbFixture);
        _cosmosDbFixture = cosmosDbFixture;
    }

    protected override async Task OnInitializeAsync(IServiceCollection services)
    {
        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.ClearDatabaseAsync());
        services.AddNewsArticleDependencies();
    }

    [Theory]
    [InlineData(NewsArticleSearchFilter.Published)]
    public async Task GetNewsArticlesUseCase_Returns_Articles_When_HandleRequest(NewsArticleSearchFilter filter)
    {
        //Arrange
        List<NewsArticleDto> seededDTOs = NewsArticleDtoTestDoubles.Generate(count: 10, predicateToFulfil: article => filter switch
        {
            NewsArticleSearchFilter.Published => article.Published,
            _ => throw new NotImplementedException()
        });

        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.WriteManyAsync(containerName: "news", seededDTOs));

        GetNewsArticlesRequest request = new(newsArticleSearchFilter: filter);

        // Act
        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> sut = ResolveApplicationType<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>>()!;

        GetNewsArticlesResponse response = await sut.HandleRequestAsync(request);

        // Assert
        IMapper<NewsArticleDto, NewsArticle> testMapper = MapNewsArticleDtoToArticleTestMapper.Create();

        List<NewsArticle> expectedArticlesOutput =
            seededDTOs.Select(testMapper.Map)
                .FilterRequestedArticles(filter)
                .OrderByDescending(t => t.Pinned)
                .ThenByDescending(t => t.ModifiedDate)
                .ToList();

        Assert.NotNull(response);
        Assert.NotNull(response.NewsArticles);
        Assert.NotEmpty(response.NewsArticles);
        Assert.Equal(expectedArticlesOutput, response.NewsArticles);
    }
}

internal static class GetNewsArticleUseCaseNewsArticleExtensions
{
    internal static IEnumerable<NewsArticle> FilterRequestedArticles(this IEnumerable<NewsArticle> input, NewsArticleSearchFilter filter)
    {
        return filter switch
        {
            NewsArticleSearchFilter.Published =>
                input.Where(t => t.Published),
            NewsArticleSearchFilter.NotPublished =>
                input.Where(t => !t.Published),
            NewsArticleSearchFilter.PublishedAndNotPublished =>
                input.Where(t => t.Published || !t.Published),
            _ => input
        };
    }
}
