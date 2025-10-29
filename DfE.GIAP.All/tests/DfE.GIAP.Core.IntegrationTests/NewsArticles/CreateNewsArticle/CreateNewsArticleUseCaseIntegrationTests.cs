using System.Diagnostics;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.CreateNewsArticle;
public sealed class CreateNewsArticleUseCaseIntegrationTests : BaseIntegrationTest
{
    private readonly CosmosDbFixture _cosmosDbFixture;

    public CreateNewsArticleUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture)
    {
        ArgumentNullException.ThrowIfNull(cosmosDbFixture);
        _cosmosDbFixture = cosmosDbFixture;
    }

    protected override async Task OnInitializeAsync(IServiceCollection services)
    {
        await _cosmosDbFixture.Database.ClearDatabaseAsync();
        services.AddNewsArticleDependencies();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task CreateNewsArticles_Creates_Article(bool isPublished, bool isPinned)
    {
        // Arrange
        IUseCaseRequestOnly<CreateNewsArticleRequest> sut = ResolveTypeFromScopedContext<IUseCaseRequestOnly<CreateNewsArticleRequest>>()!;

        const string stubArticleTitle = "Test title";
        const string stubArticleBody = "Test body";

        CreateNewsArticleRequest request = new(
            stubArticleTitle,
            stubArticleBody,
            Published: isPublished,
            Pinned: isPinned);

        Stopwatch watch = Stopwatch.StartNew();
        DateTime preRequestCreationDate = DateTime.UtcNow;

        // Act
        await sut.HandleRequestAsync(request);
        watch.Stop();

        // Assert
        IEnumerable<NewsArticleDto> enumerable = await _cosmosDbFixture.Database.ReadManyAsync<NewsArticleDto>();
        NewsArticleDto newsArticleDto = Assert.Single(enumerable);

        Assert.False(string.IsNullOrEmpty(newsArticleDto.id));
        Assert.Equal(stubArticleTitle, newsArticleDto.Title);
        Assert.Equal(stubArticleBody, newsArticleDto.Body);
        Assert.Equal(isPublished, newsArticleDto.Published);
        Assert.Equal(isPinned, newsArticleDto.Pinned);

        Assert.InRange(newsArticleDto.CreatedDate, preRequestCreationDate, preRequestCreationDate + watch.Elapsed);
        Assert.InRange(newsArticleDto.ModifiedDate, preRequestCreationDate, preRequestCreationDate + watch.Elapsed);

    }
}
