using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.GetNewsArticlesById;
public sealed class GetNewsArticleByIdUseCaseIntegrationTests : BaseIntegrationTest
{
    private readonly GiapCosmosDbFixture _cosmosDbFixture;

    public GetNewsArticleByIdUseCaseIntegrationTests(GiapCosmosDbFixture cosmosDbFixture)
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

    [Fact]
    public async Task GetNewsArticleByIdUseCase_Returns_Article_When_HandleRequest()
    {
        // Arrange
        IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> sut = ResolveApplicationType<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>>()!;

        // Seed articles
        List<NewsArticleDto> seededArticles = NewsArticleDtoTestDoubles.Generate();

        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.WriteManyAsync(containerName: "news", seededArticles));

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
        IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> sut = ResolveApplicationType<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>>()!;

        // Seed articles
        List<NewsArticleDto> seededArticles = NewsArticleDtoTestDoubles.Generate();

        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.WriteManyAsync(containerName: "news", seededArticles));

        GetNewsArticleByIdRequest request = new(Id: NewsArticleIdentifier.New());

        // Act
        GetNewsArticleByIdResponse response = await sut.HandleRequestAsync(request);

        //Assert
        Assert.NotNull(response);
        Assert.Null(response.NewsArticle);
    }
}
