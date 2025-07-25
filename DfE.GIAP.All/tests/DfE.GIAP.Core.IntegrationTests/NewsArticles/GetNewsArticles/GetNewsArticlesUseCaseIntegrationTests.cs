using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Enums;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.GetNewsArticles;

[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetNewsArticlesUseCaseIntegrationTests : BaseIntegrationTest
{
    public GetNewsArticlesUseCaseIntegrationTests(CosmosDbFixture fixture) : base(fixture)
    {
    }

    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        services.AddNewsArticleDependencies();
        return Task.CompletedTask;
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

        await Fixture.Database.WriteManyAsync(seededDTOs);

        GetNewsArticlesRequest request = new(newsArticleSearchFilter: filter);

        // Act
        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> sut = ResolveTypeFromScopedContext<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>>()!;

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
