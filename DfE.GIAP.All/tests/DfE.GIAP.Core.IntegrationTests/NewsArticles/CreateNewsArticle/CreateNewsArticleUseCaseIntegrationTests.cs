using System.Diagnostics;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.CreateNewsArticle;
public sealed class CreateNewsArticleUseCaseIntegrationTests : BaseIntegrationTest
{
    private readonly GiapCosmosDbFixture _cosmosDbFixture;

    public CreateNewsArticleUseCaseIntegrationTests(GiapCosmosDbFixture cosmosDbFixture)
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
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task CreateNewsArticles_Creates_Article(bool isPublished, bool isPinned)
    {
        // Arrange
        IUseCaseRequestOnly<CreateNewsArticleRequest> sut = ResolveApplicationType<IUseCaseRequestOnly<CreateNewsArticleRequest>>()!;

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
        List<NewsArticleDto> enumerable =
            await _cosmosDbFixture.InvokeAsync(
                databaseName: _cosmosDbFixture.DatabaseName,
                (client) => client.ReadManyAsync<NewsArticleDto>(containerName: "news"));

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
