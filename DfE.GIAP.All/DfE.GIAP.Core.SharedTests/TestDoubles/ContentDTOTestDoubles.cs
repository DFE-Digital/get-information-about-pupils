using Bogus;
using DfE.GIAP.Core.Contents.Infrastructure.Repositories;

namespace DfE.GIAP.Core.SharedTests.TestDoubles;
public static class ContentDtoTestDoubles
{
    private const int CONTENT_DOCUMENTTYPE = 20;
    public static List<ContentDto> Generate(int count = 10)
    {
        List<ContentDto> contentDtos = [];

        for (int index = 0; index < count; index++)
        {
            Faker<ContentDto> faker = CreateGenerator();
            contentDtos.Add(
                faker.Generate());
        }

        return contentDtos;
    }

    private static Faker<ContentDto> CreateGenerator()
    {
        return new Faker<ContentDto>()
            .StrictMode(true)
            .RuleFor(t => t.id, (f) => f.Random.Guid().ToString())
            .RuleFor(t => t.Title, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.Body, (f) => f.Lorem.Paragraphs())
            .RuleFor(t => t.DOCTYPE, (f) => CONTENT_DOCUMENTTYPE);
    }
}
