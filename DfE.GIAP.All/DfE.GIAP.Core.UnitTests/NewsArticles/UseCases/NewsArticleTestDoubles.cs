using Bogus;
using DfE.GIAP.Core.UnitTests.Extensions;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases;

internal static class NewsArticleTestDoubles
{
    internal static NewsArticle Create()
    {
        Faker<NewsArticle> generator = CreateGenerator();
        return generator.Generate(1).Single();
    }

    private static Faker<NewsArticle> CreateGenerator()
    {
        return new Faker<NewsArticle>()
            .UseSeed(13487123)
            .StrictMode(true)
            .RuleFor((target) => target.Id, (faker) => NewsArticleIdentifier.From(faker.Lorem.Word()))
            .RuleFor((target) => target.DraftTitle, (faker) => faker.Lorem.Words().Merge())
            .RuleFor((target) => target.DraftBody, (faker) => faker.Lorem.Words().Merge())
            .RuleFor((target) => target.Title, (faker) => faker.Lorem.Words().Merge())
            .RuleFor((target) => target.Body, (faker) => faker.Lorem.Words().Merge())
            .RuleFor((target) => target.CreatedDate, (faker) => faker.Date.Recent())
            .RuleFor((target) => target.ModifiedDate, (faker) => faker.Date.Recent())
            .RuleFor((target) => target.Archived, (faker) => faker.Random.Bool())
            .RuleFor((target) => target.Pinned, (faker) => faker.Random.Bool())
            .RuleFor((target) => target.Published, (faker) => faker.Random.Bool());
    }
}
