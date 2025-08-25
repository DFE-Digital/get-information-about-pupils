using System.Diagnostics;
using System.Net;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles.UpdateNewsArticle;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class UpdateNewsArticleUseCaseIntegrationTests : BaseIntegrationTest
{
    public UpdateNewsArticleUseCaseIntegrationTests(CosmosDbFixture fixture) : base(fixture) { }

    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        services.AddNewsArticleDependencies();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task UpdateDataThatDoesntExist()
    {
        // Arrange 
        string unknownArticleId = Guid.NewGuid().ToString();
        UpdateNewsArticlesRequestProperties requestProperties = new(id: unknownArticleId);
        UpdateNewsArticleRequest request = new(requestProperties);

        IUseCaseRequestOnly<UpdateNewsArticleRequest> sut = ResolveTypeFromScopedContext<IUseCaseRequestOnly<UpdateNewsArticleRequest>>();

        // Act Assert
        Func<Task> act = () => sut.HandleRequestAsync(request);
        CosmosException cosmosException = await Assert.ThrowsAsync<CosmosException>(act);
        Assert.Equal(HttpStatusCode.NotFound, cosmosException.StatusCode);
    }


    [Theory]
    [MemberData(nameof(ArticleVariants))]
    public async Task UpdateNullableRecordSuccessfully(NewsArticleDto seededArticle, bool requestPinned, bool requestPublished)
    {
        // Arrange
        await Fixture.Database.WriteItemAsync(seededArticle);
        DateTime beforeRequestCreationDateTimeUtc = DateTime.UtcNow;
        Stopwatch stopWatch = Stopwatch.StartNew();

        UpdateNewsArticlesRequestProperties requestProperties = new(seededArticle.id)
        {
            Title = SanitisedTextResult.From("Test tile"),
            Body = SanitisedTextResult.From("Test body"),
            Pinned = requestPinned,
            Published = requestPublished,
        };

        UpdateNewsArticleRequest request = new(requestProperties);

        IUseCaseRequestOnly<UpdateNewsArticleRequest> sut = ResolveTypeFromScopedContext<IUseCaseRequestOnly<UpdateNewsArticleRequest>>();

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        stopWatch.Stop();

        IEnumerable<string> updatedArticleIdentifier = [seededArticle.id];
        IEnumerable<NewsArticleDto?> articles = await Fixture.Database.ReadManyAsync<NewsArticleDto>(updatedArticleIdentifier);
        NewsArticleDto? updatedArticle = Assert.Single(articles);

        Assert.NotNull(updatedArticle);
        Assert.Equal(seededArticle.id, updatedArticle.id);
        Assert.Equal(requestProperties.Title.Value, updatedArticle.Title);
        Assert.Equal(requestProperties.Body.Value, updatedArticle.Body);
        Assert.Equal(requestProperties.Pinned, updatedArticle.Pinned);
        Assert.Equal(requestProperties.Published, updatedArticle.Published);
        Assert.InRange(updatedArticle.CreatedDate, beforeRequestCreationDateTimeUtc, beforeRequestCreationDateTimeUtc.Add(stopWatch.Elapsed));
        Assert.InRange(updatedArticle.ModifiedDate, beforeRequestCreationDateTimeUtc, beforeRequestCreationDateTimeUtc.Add(stopWatch.Elapsed));
    }

    public static IEnumerable<object[]> ArticleVariants =>
     new List<object[]>
     {
         new object[] { NewsArticleDtoTestDoubles.Generate(count: 1)[0], true, false },
         new object[] { NewsArticleDtoTestDoubles.Generate(count: 1)[0], true, true },
         new object[] { NewsArticleDtoTestDoubles.GenerateEmpty(), false, true },
     };

}
