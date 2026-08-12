using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.DeleteNewsArticles;

public sealed class DeleteNewsArticlesUseCaseIntegrationTests : BaseIntegrationTest
{

    public DeleteNewsArticlesUseCaseIntegrationTests(GiapCosmosDbFixture cosmosDbFixture)
        : base(cosmosDbFixture)
    {
    }

    protected override async Task OnInitializeAsync(IServiceCollection services)
    {
        await CosmosDb.InvokeAsync(
            databaseName: CosmosDb.DatabaseName, (client) => client.ClearDatabaseAsync());

        services.AddNewsArticleDependencies();
    }

    [Fact]
    public async Task DeleteNewsArticles_Deletes_SelectedArticle()
    {
        // Arrange
        IUseCaseRequestOnly<DeleteNewsArticleRequest> sut = ResolveApplicationType<IUseCaseRequestOnly<DeleteNewsArticleRequest>>()!;

        // Seed articles
        const int countGenerated = 10;
        List<NewsArticleDto> seededArticles = NewsArticleDtoTestDoubles.Generate(countGenerated);
        await CosmosDb.InvokeAsync(
            databaseName: CosmosDb.DatabaseName,
            (client) => client.WriteManyAsync(containerName: "news", seededArticles));

        NewsArticleDto targetDeleteArticle = seededArticles[0];
        DeleteNewsArticleRequest request = new(Id: NewsArticleIdentifier.From(targetDeleteArticle.id));

        // Act
        await sut.HandleRequestAsync(request);

        //Assert
        IEnumerable<NewsArticleDto> newsArticleDtosShouldReturn = seededArticles.Where(t => t.id != targetDeleteArticle.id);

        List<NewsArticleDto> queriedArticles =
            await CosmosDb.InvokeAsync(
                databaseName: CosmosDb.DatabaseName,
                (client) => client.ReadManyAsync<NewsArticleDto>(containerName: "news"));

        Assert.Equivalent(newsArticleDtosShouldReturn, queriedArticles);
        Assert.Equal(countGenerated - 1, queriedArticles.Count(t => t != null));
    }
}
