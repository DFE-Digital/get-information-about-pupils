using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetNewsArticleByIdUseCaseIntegrationTests : IAsyncLifetime
{
    private readonly CosmosDbFixture _fixture;

    public GetNewsArticleByIdUseCaseIntegrationTests(CosmosDbFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync() => await _fixture.Database.ClearDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetNewsArticleByIdUseCase_Returns_Article_When_HandleRequest()
    {
        // Arrange
        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddSharedDependencies()
                .AddNewsArticleDependencies();
        IServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> sut =
            scope.ServiceProvider.GetService<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>>()!;

        // Seed articles
        List<NewsArticleDto> seededArticles = NewsArticleDtoTestDoubles.Generate();
        await _fixture.Database.WriteManyAsync(seededArticles);

        NewsArticleDto targetArticle = seededArticles[0];
        GetNewsArticleByIdRequest request = new(Id: targetArticle.id);

        // Act
        GetNewsArticleByIdResponse response = await sut.HandleRequestAsync(request);

        //Assert
        IMapper<NewsArticleDto, NewsArticle> testMapper = MapNewsArticleDtoToArticleTestMapper.Create();
        NewsArticle seededTargetArticle = testMapper.Map(targetArticle);
        Assert.NotNull(response);
        Assert.NotNull(response.NewsArticle);
        Assert.Equal(seededTargetArticle, response.NewsArticle);
    }

    [Fact]
    public async Task GetNewsArticleByIdUseCase_Returns_Null_When_HandleRequest_Finds_NoArticleMatchingId()
    {
        // Arrange
        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddSharedDependencies()
                .AddNewsArticleDependencies();
        IServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> sut =
            scope.ServiceProvider.GetService<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>>()!;

        // Seed articles
        List<NewsArticleDto> seededArticles = NewsArticleDtoTestDoubles.Generate();
        await _fixture.Database.WriteManyAsync(seededArticles);

        string unknownArticleId = Guid.NewGuid().ToString();
        GetNewsArticleByIdRequest request = new(Id: unknownArticleId);

        // Act
        GetNewsArticleByIdResponse response = await sut.HandleRequestAsync(request);

        //Assert
        Assert.NotNull(response);
        Assert.Null(response.NewsArticle);

    }
}
