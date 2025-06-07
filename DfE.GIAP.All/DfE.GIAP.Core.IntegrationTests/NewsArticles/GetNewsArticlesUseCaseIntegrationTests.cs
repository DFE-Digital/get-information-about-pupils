using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles;

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
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task GetNewsArticlesUseCase_Returns_Articles_When_HandleRequest(bool isArchived, bool isDraft)
    {
        // Arrange
        await _fixture.Database.ClearDatabaseAsync();

        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddTestServices()
                .AddNewsArticleDependencies();

        IServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        List<NewsArticleDTO> seededDTOs = NewsArticleDTOTestDoubles.Generate(count: 10);

        await Parallel.ForEachAsync(seededDTOs, async (dto, ct) => await _fixture.Database.WriteAsync(dto));

        GetNewsArticlesRequest request = new(
            IsArchived: isArchived,
            IsDraft: isDraft);

        // Act
        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> sut =
            scope.ServiceProvider.GetService<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>>()!;
        GetNewsArticlesResponse response = await sut.HandleRequest(request);

        // Assert
        IMapper<NewsArticleDTO, NewsArticle> testMapper = TestMapNewsArticleDTOToArticle.Create();

        List<NewsArticle> expectedArticlesOutput =
            seededDTOs.Select(testMapper.Map)
                .FilterRequestedArticles(request.IsArchived, request.IsDraft)
                .OrderArticles()
                .ToList();

        Assert.NotNull(response);
        Assert.NotNull(response.NewsArticles);
        Assert.NotEmpty(response.NewsArticles);
        Assert.Equal(expectedArticlesOutput, response.NewsArticles);
    }
}

internal static class GetNewsArticleUseCaseNewsArticleExtensions
{

    internal static IEnumerable<NewsArticle> FilterRequestedArticles(this IEnumerable<NewsArticle> input, bool requestIsArchived, bool requestIsDraft)
        => input
            .Where(t => t.Archived == requestIsArchived) // if requested archived include
            .Where(t => t.Published != requestIsDraft); // if requested draft then include
    
    internal static IEnumerable<NewsArticle> OrderArticles(this IEnumerable<NewsArticle> input)
        => input
                .OrderByDescending(t => t.Pinned)
                .ThenByDescending(t => t.ModifiedDate);
}
