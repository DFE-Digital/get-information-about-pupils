using Bogus;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.Core.SharedTests.TestDoubles;

public static class NewsArticleDtoTestDoubles
{
    public static List<NewsArticleDto> Generate(int count = 10, Func<NewsArticleDto, bool>? predicateToFulfil = null)
    {
        List<NewsArticleDto> articles = [];

        Faker<NewsArticleDto> faker = CreateGenerator();

        const int circuitBreakerGenerationCounter = 111_111;

        for (int attempts = 0; articles.Count < count && attempts < circuitBreakerGenerationCounter; attempts++)
        {
            NewsArticleDto dto = faker.Generate();
            if (predicateToFulfil is null || predicateToFulfil(dto))
            {
                articles.Add(dto);
            }
        }

        if (articles.Count < count)
        {
            throw new ArgumentException($"Unable to generate {count} DTOs after {circuitBreakerGenerationCounter} attempts.");
        }

        return articles;
    }

    public static NewsArticleDto GenerateEmpty() => new()
    {
        id = Guid.NewGuid().ToString(),
        Title = null!,
        Body = null!,
    };

    private static Faker<NewsArticleDto> CreateGenerator()
    {
        return new Faker<NewsArticleDto>()
            .StrictMode(true)
            .RuleFor(t => t.id, (f) => f.Random.Guid().ToString())
            .RuleFor(t => t.Title, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.Body, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.Published, (f) => f.Random.Bool())
            .RuleFor(t => t.Pinned, (f) => f.Random.Bool())
            .RuleFor(t => t.CreatedDate, (f) => f.Date.Recent())
            .RuleFor(t => t.ModifiedDate, (f) => f.Date.Recent());
    }
}
