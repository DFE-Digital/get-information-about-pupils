using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Client.Java;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles;
internal sealed class MapNewsArticleDtoToArticleTestMapper : IMapper<NewsArticleDto, NewsArticle>
{
    public NewsArticle Map(NewsArticleDto input)
    {
        return new()
        {
            Id = NewsArticleIdentifier.From(input.id),
            Title = input.Title,
            Body = input.Body,
            Pinned = input.Pinned,
            Published = input.Published,
            CreatedDate = input.CreatedDate,
            ModifiedDate = input.ModifiedDate
        };
    }

    public static IMapper<NewsArticleDto, NewsArticle> Create() => new MapNewsArticleDtoToArticleTestMapper();
}
