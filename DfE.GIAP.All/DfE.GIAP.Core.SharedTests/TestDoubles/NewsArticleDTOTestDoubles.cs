using Bogus;

namespace DfE.GIAP.Core.SharedTests.TestDoubles;

public static class NewsArticleDTOTestDoubles
{
    private const int NEWS_ARTICLES_DOCUMENT_TYPE = 7;
    public static List<NewsArticleDTO> Generate(int count = 10)
    {
        List<NewsArticleDTO> articles = [];

        for (int index = 0; index < count; index++)
        {
            Faker<NewsArticleDTO> faker = CreateGenerator();
            articles.Add(
                faker.Generate());
        }

        return articles;
    }

    private static Faker<NewsArticleDTO> CreateGenerator()
    {
        return new Faker<NewsArticleDTO>()
            .StrictMode(true)
            .RuleFor(t => t.Pinned, (f) => f.Random.Bool())
            .RuleFor(t => t.Archived, (f) => f.Random.Bool())
            .RuleFor(t => t.Published, (f) => f.Random.Bool())
            .RuleFor(t => t.Id, (f) => f.Random.Guid().ToString())
            .RuleFor(t => t.DraftBody, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.DraftTitle, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.ModifiedDate, (f) => f.Date.Recent())
            .RuleFor(t => t.CreatedDate, (f) => f.Date.Recent())
            .RuleFor(t => t.Title, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.Body, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.DOCTYPE, (f) => NEWS_ARTICLES_DOCUMENT_TYPE);
    }
}
