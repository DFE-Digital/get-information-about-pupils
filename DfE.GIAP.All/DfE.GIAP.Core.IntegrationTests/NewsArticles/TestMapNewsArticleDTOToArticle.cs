using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Core.IntegrationTests.NewsArticles;
internal sealed class TestMapNewsArticleDTOToArticle : IMapper<NewsArticleDTO, NewsArticle>
{
    public NewsArticle Map(NewsArticleDTO input)
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

    public static IMapper<NewsArticleDTO, NewsArticle> Create() => new TestMapNewsArticleDTOToArticle();
}
