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
        //Arrange
        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddSharedDependencies()
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
        IMapper<NewsArticleDTO, NewsArticle> testMapper = MapNewsArticleDTOToArticleTestMapper.Create();

        List<NewsArticle> expectedArticlesOutput =
            seededDTOs.Select(testMapper.Map)
                .Where(t => t.Archived == isArchived) // if requested archived include
                .Where(t => t.Published != isDraft) // if requested draft then include
                .OrderByDescending(t => t.Pinned)
                .ThenByDescending(t => t.ModifiedDate)
                .ToList();

        Assert.NotNull(response);
        Assert.NotNull(response.NewsArticles);
        Assert.NotEmpty(response.NewsArticles);
        Assert.Equal(expectedArticlesOutput, response.NewsArticles);
    }
}
