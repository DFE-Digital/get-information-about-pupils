using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.GetNewsArticlesById;

[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetNewsArticleByIdUseCaseIntegrationTests : BaseIntegrationTest
{
    private CosmosDbFixture Fixture { get; }

    public GetNewsArticleByIdUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture) : base()
    {
        Fixture = cosmosDbFixture;
    }

    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        services
            .AddNewsArticleDependencies();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetNewsArticleByIdUseCase_Returns_Article_When_HandleRequest()
    {
        // Arrange
        IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> sut =
            ResolveTypeFromScopedContext<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>>()!;

        // Seed articles
        List<NewsArticleDto> seededArticles = NewsArticleDtoTestDoubles.Generate();
        await Fixture.Database.WriteManyAsync(seededArticles);

        NewsArticleDto targetArticle = seededArticles[0];
        GetNewsArticleByIdRequest request = new(Id: NewsArticleIdentifier.From(targetArticle.id));

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
        IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> sut =
            ResolveTypeFromScopedContext<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>>()!;

        // Seed articles
        List<NewsArticleDto> seededArticles = NewsArticleDtoTestDoubles.Generate();
        await Fixture.Database.WriteManyAsync(seededArticles);

        GetNewsArticleByIdRequest request = new(Id: NewsArticleIdentifier.New());

        // Act
        GetNewsArticleByIdResponse response = await sut.HandleRequestAsync(request);

        //Assert
        Assert.NotNull(response);
        Assert.Null(response.NewsArticle);
    }
}
