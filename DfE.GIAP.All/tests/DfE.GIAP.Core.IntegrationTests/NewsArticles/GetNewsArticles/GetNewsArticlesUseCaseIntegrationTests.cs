using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Enums;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.GetNewsArticles;

[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetNewsArticlesUseCaseIntegrationTests : IAsyncLifetime
{
    private readonly CosmosDbFixture _fixture;

    public GetNewsArticlesUseCaseIntegrationTests(CosmosDbFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync() => await _fixture.Database.ClearDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData(NewsArticleSearchFilter.ArchivedWithPublished)]
    [InlineData(NewsArticleSearchFilter.NotArchivedWithPublished)]
    public async Task GetNewsArticlesUseCase_Returns_Articles_When_HandleRequest(NewsArticleSearchFilter filter)
    {
        //Arrange
        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddSharedDependencies()
                .AddNewsArticleDependencies();

        IServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        List<NewsArticleDto> seededDTOs = NewsArticleDtoTestDoubles.Generate(count: 10, predicateToFulfil: article => filter switch
        {
            NewsArticleSearchFilter.ArchivedWithPublished => article.Archived && article.Published,
            NewsArticleSearchFilter.NotArchivedWithPublished => !article.Archived && article.Published,
            _ => throw new NotImplementedException()
        });

        await _fixture.Database.WriteManyAsync(seededDTOs);

        GetNewsArticlesRequest request = new(newsArticleSearchFilter: filter);

        // Act
        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> sut =
            scope.ServiceProvider.GetService<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>>()!;
        GetNewsArticlesResponse response = await sut.HandleRequestAsync(request);

        // Assert
        IMapper<NewsArticleDto, NewsArticle> testMapper = MapNewsArticleDtoToArticleTestMapper.Create();

        List<NewsArticle> expectedArticlesOutput =
            seededDTOs.Select(testMapper.Map)
                .FilterRequestedArticles(filter)
                .OrderArticles(filter)
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
            NewsArticleSearchFilter.ArchivedWithPublished =>
                input.Where(t => t.Archived && t.Published),
            NewsArticleSearchFilter.ArchivedWithNotPublished =>
                input.Where(t => t.Archived && !t.Published),
            NewsArticleSearchFilter.ArchivedWithPublishedAndNotPublished =>
                input.Where(t => t.Archived),
            NewsArticleSearchFilter.NotArchivedWithPublished =>
                input.Where(t => !t.Archived && t.Published),
            NewsArticleSearchFilter.NotArchivedWithNotPublished =>
                input.Where(t => !t.Archived && !t.Published),
            NewsArticleSearchFilter.NotArchivedWithPublishedAndNotPublished =>
                input.Where(t => !t.Archived),
            _ => input
        };
    }

    internal static IEnumerable<NewsArticle> OrderArticles(this IEnumerable<NewsArticle> input, NewsArticleSearchFilter filter)
    {
        return filter switch
        {
            NewsArticleSearchFilter.NotArchivedWithPublished
            or NewsArticleSearchFilter.NotArchivedWithNotPublished
            or NewsArticleSearchFilter.NotArchivedWithPublishedAndNotPublished =>
                input.OrderByDescending(t => t.Pinned)
                     .ThenByDescending(t => t.ModifiedDate),
            _ => input.OrderByDescending(t => t.ModifiedDate)
        };
    }
}
