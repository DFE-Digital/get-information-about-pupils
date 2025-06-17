using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.Mappers;

/// <summary>
/// Maps a NewsArticleDTO object to a NewsArticle entity.
/// </summary>
internal class NewsArticleDtoToEntityMapper : IMapper<NewsArticleDTO, NewsArticle>
{
    /// <summary>
    /// Converts a NewsArticleDTO object into a NewsArticle entity.
    /// </summary>
    /// <param name="input">The DTO containing raw article data.</param>
    /// <returns>A NewsArticle entity populated with the DTO values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the input DTO is null.</exception>
    public NewsArticle Map(NewsArticleDTO input)
    {
        // Validate input to prevent null reference issues.
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input), "Mapping input cannot be null.");
        }

        // Create a new NewsArticle entity and populate it with the DTO data.
        return new NewsArticle
        {
            Id = NewsArticleIdentifier.From(input.Id),
            Title = input.Title,
            Body = input.Body,
            DraftBody = input.DraftBody,
            DraftTitle = input.DraftTitle,
            Published = input.Published,
            Archived = input.Archived,
            Pinned = input.Pinned,
            CreatedDate = input.CreatedDate,
            ModifiedDate = input.ModifiedDate
        };
    }
}
