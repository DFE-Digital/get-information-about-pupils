using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.DeleteNewsArticles;

[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class DeleteNewsArticlesUseCaseIntegrationTests : BaseIntegrationTest
{
    private CosmosDbFixture Fixture { get; }

    public DeleteNewsArticlesUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture) : base()
    {
        Fixture = cosmosDbFixture;
    }

    protected async override Task OnInitializeAsync(IServiceCollection services)
    {
        await Fixture.Database.ClearDatabaseAsync();

        services
            .AddCosmosDbDependencies()
            .AddNewsArticleDependencies();

        //return Task.CompletedTask;
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
