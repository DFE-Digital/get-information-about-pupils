using System.Diagnostics;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.CreateNewsArticle;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class CreateNewsArticleUseCaseIntegrationTests : IAsyncLifetime
{
    private readonly CosmosDbFixture _fixture;

    public CreateNewsArticleUseCaseIntegrationTests(CosmosDbFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync() => await _fixture.Database.ClearDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    public async Task MyTest(bool isPublished, bool isArchived, bool isPinned)
    {
        // Arrange
        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddSharedDependencies()
                .AddNewsArticleDependencies();

        IServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IUseCaseRequestOnly<CreateNewsArticleRequest> sut =
            scope.ServiceProvider.GetService<IUseCaseRequestOnly<CreateNewsArticleRequest>>()!;

        CreateNewsArticleRequest request = new(
            "Test title",
            "Test body",
            Published: isPublished,
            Archived: isArchived,
            Pinned: isPinned);

        Stopwatch watch = Stopwatch.StartNew();
        DateTime preRequestCreationDate = DateTime.UtcNow;

        // Act
        await sut.HandleRequestAsync(request);
        watch.Stop();

        // Assert
        IEnumerable<NewsArticleDto> enumerable = await _fixture.Database.ReadManyAsync<NewsArticleDto>();
        NewsArticleDto newsArticleDto = Assert.Single(enumerable);

        Assert.False(string.IsNullOrEmpty(newsArticleDto.id));
        Assert.Equal("Test title", newsArticleDto.Title);
        Assert.Equal("Test body", newsArticleDto.Body);
        Assert.Equal(isPublished, newsArticleDto.Published);
        Assert.Equal(isArchived, newsArticleDto.Archived);
        Assert.Equal(isPinned, newsArticleDto.Pinned);
        Assert.Equal(7, newsArticleDto.DOCTYPE);

        Assert.Equal(string.Empty, newsArticleDto.DraftTitle);
        Assert.Equal(string.Empty, newsArticleDto.DraftBody);
        
        Assert.InRange(newsArticleDto.CreatedDate, preRequestCreationDate, preRequestCreationDate + watch.Elapsed);
        Assert.InRange(newsArticleDto.ModifiedDate, preRequestCreationDate, preRequestCreationDate + watch.Elapsed);
        
    }    
}
