using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using DfE.GIAP.SharedTests.TestDoubles.News;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.GetNewsArticlesById;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetNewsArticleByIdUseCaseIntegrationTests : BaseIntegrationTest
{
    public GetNewsArticleByIdUseCaseIntegrationTests(CosmosDbFixture fixture) : base(fixture) { }


    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        services.AddNewsArticleDependencies();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetNewsArticleByIdUseCase_Returns_Article_When_HandleRequest()
    {
        // Arrange
        IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> sut = ResolveTypeFromScopedContext<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>>()!;

        // Seed articles
        List<NewsArticleDto> seededArticles = NewsArticleDtoTestDoubles.Generate();
        await CosmosDbFixture.Database.WriteManyAsync(seededArticles);

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
        IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> sut = ResolveTypeFromScopedContext<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>>()!;

        // Seed articles
        List<NewsArticleDto> seededArticles = NewsArticleDtoTestDoubles.Generate();
        await CosmosDbFixture.Database.WriteManyAsync(seededArticles);

        string unknownArticleId = Guid.NewGuid().ToString();
        GetNewsArticleByIdRequest request = new(Id: unknownArticleId);

        // Act
        GetNewsArticleByIdResponse response = await sut.HandleRequestAsync(request);

        //Assert
        Assert.NotNull(response);
        Assert.Null(response.NewsArticle);

    }
}
