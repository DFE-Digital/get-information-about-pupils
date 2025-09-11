using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.DeleteNewsArticles;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class DeleteNewsArticlesUseCaseIntegrationTests : BaseIntegrationTest
{
    public DeleteNewsArticlesUseCaseIntegrationTests(CosmosDbFixture fixture) : base(fixture) { }

    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        services.AddNewsArticleDependencies();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task DeleteNewsArticles_Deletes_SelectedArticle()
    {
        // Arrange
        IUseCaseRequestOnly<DeleteNewsArticleRequest> sut = ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeleteNewsArticleRequest>>()!;

        // Seed articles
        const int countGenerated = 10;
        List<NewsArticleDto> seededArticles = NewsArticleDtoTestDoubles.Generate(countGenerated);
        await Fixture.Database.WriteManyAsync(seededArticles);

        NewsArticleDto targetDeleteArticle = seededArticles[0];
        DeleteNewsArticleRequest request = new(Id: NewsArticleIdentifier.From(targetDeleteArticle.id));

        // Act
        await sut.HandleRequestAsync(request);

        //Assert
        IEnumerable<NewsArticleDto> newsArticleDtosShouldReturn = seededArticles.Where(t => t.id != targetDeleteArticle.id);
        IEnumerable<NewsArticleDto?> queriedArticles = await Fixture.Database.ReadManyAsync<NewsArticleDto>();

        Assert.Equivalent(newsArticleDtosShouldReturn, queriedArticles);
        Assert.Equal(countGenerated - 1, queriedArticles.Count(t => t != null));
    }
}
