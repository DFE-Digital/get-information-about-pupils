using DfE.GIAP.Core.NewsArticles;
using DfE.GIAP.Core.SharedTests;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles;

[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetNewsArticlesUseCaseIntegrationTests
{
    private readonly CosmosDbFixture _fixture;

    public GetNewsArticlesUseCaseIntegrationTests(CosmosDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task GetNewsArticlesUseCase_Returns_Articles_When_HandleRequest(bool isArchived, bool isDraft)
    {
        // Arrange
        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddTestServices()
                .AddNewsArticleDependencies();

        IServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> sut =
            scope.ServiceProvider.GetService<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>>()!;

        List<NewsArticleDTO> seededDTOs = NewsArticleDTOTestDoubles.Generate();

        await Parallel.ForEachAsync(seededDTOs, async (dto, ct) => await _fixture.Database.WriteAsync(dto));

        GetNewsArticlesRequest request = new(
            IsArchived: isArchived,
            IsDraft: isDraft);

        // Act
        GetNewsArticlesResponse response = await sut.HandleRequest(request);

        // Assert
        Assert.NotNull(response);

        List<NewsArticle> expectedOutput =
            MapDTOToApplicationModel(seededDTOs)
                .FilterRequestedArticles(request.IsArchived, request.IsDraft)
                .OrderArticles()
                .ToList();

        Assert.Equal(expectedOutput, response.NewsArticles);
    }

    private static List<NewsArticle> MapDTOToApplicationModel(IEnumerable<NewsArticleDTO> input)
    {
        return input.Select(Map).ToList();

        static NewsArticle Map(NewsArticleDTO input) => new()
        {
            Id = input.ID,
            Title = input.Title,
            Body = input.Body,
            Archived = input.Archived,
            Pinned = input.Pinned,
            Published = input.Published,
            DraftTitle = input.DraftTitle,
            DraftBody = input.DraftBody,
            CreatedDate = input.CreatedDate,
            ModifiedDate = input.ModifiedDate
        };
    }
}

internal static class GetNewsArticleUseCaseNewsArticleExtensions
{

    internal static IEnumerable<NewsArticle> FilterRequestedArticles(this IEnumerable<NewsArticle> input, bool requestIsArchived, bool requestIsDraft)
        => input
            .Where(t => t.Archived = requestIsArchived) // if requested archived include
            .Where(t => t.Published = !requestIsDraft); // if requested draft then include
    
    internal static IEnumerable<NewsArticle> OrderArticles(this IEnumerable<NewsArticle> input)
        => input
                .OrderByDescending(t => t.Pinned)
                .ThenBy(t => t.ModifiedDate);
}
