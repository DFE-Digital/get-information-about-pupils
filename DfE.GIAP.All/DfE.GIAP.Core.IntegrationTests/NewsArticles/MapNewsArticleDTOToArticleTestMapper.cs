using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles;
internal sealed class MapNewsArticleDtoToArticleTestMapper : IMapper<NewsArticleDto, NewsArticle>
{
    public NewsArticle Map(NewsArticleDto input)
    {
        return new()
        {
            Id = NewsArticleIdentifier.From(input.Id),
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

    public static IMapper<NewsArticleDto, NewsArticle> Create() => new MapNewsArticleDtoToArticleTestMapper();
}
