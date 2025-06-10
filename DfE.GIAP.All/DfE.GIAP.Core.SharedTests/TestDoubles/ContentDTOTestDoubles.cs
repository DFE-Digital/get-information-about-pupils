using Bogus;
using DfE.GIAP.Core.Content.Infrastructure.Repositories;

namespace DfE.GIAP.Core.SharedTests.TestDoubles;
public static class ContentDTOTestDoubles
{
    private const int CONTENT_DOCUMENTTYPE = 20;
    public static List<ContentDTO> Generate(int count = 10)
    {
        List<ContentDTO> contentDtos = [];

        for (int index = 0; index < count; index++)
        {
            Faker<ContentDTO> faker = CreateGenerator();
            contentDtos.Add(
                faker.Generate());
        }

        return contentDtos;
    }

    private static Faker<ContentDTO> CreateGenerator()
    {
        return new Faker<ContentDTO>()
            .StrictMode(true)
            .RuleFor(t => t.id, (f) => f.Random.Guid().ToString())
            .RuleFor(t => t.Title, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.Body, (f) => f.Lorem.Paragraphs())
            .RuleFor(t => t.DOCTYPE, (f) => CONTENT_DOCUMENTTYPE);
    }
}
