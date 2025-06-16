using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.Mappers;

/// <summary>
/// Provides functionality to map a <see cref="NewsArticle"/> entity to a <see cref="NewsArticleDTO"/> data transfer
/// object.
/// </summary>
/// <remarks>This mapper is responsible for converting the properties of a <see cref="NewsArticle"/> entity into a
/// corresponding <see cref="NewsArticleDTO"/> object. It ensures that all relevant fields are transferred, including
/// metadata such as creation and modification dates.</remarks>
internal class NewsArticleEntityToDtoMapper : IMapper<NewsArticle, NewsArticleDTO>
{
    /// <summary>
    /// Maps a <see cref="NewsArticle"/> entity to a <see cref="NewsArticleDTO"/> data transfer object.
    /// </summary>
    /// <remarks>This method performs a direct mapping of properties from the <see cref="NewsArticle"/> entity
    /// to the <see cref="NewsArticleDTO"/> object. The <c>DocumentType</c> property of the resulting DTO is set to a
    /// constant value of 7.</remarks>
    /// <param name="input">The <see cref="NewsArticle"/> instance to map. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="NewsArticleDTO"/> instance populated with the corresponding data from the <paramref name="input"/>
    /// entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is <see langword="null"/>.</exception>
    public NewsArticleDTO Map(NewsArticle input)
    {
        // Validate input to prevent null reference issues.
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input), "Mapping input cannot be null.");
        }
        // Create a new NewsArticleDTO and populate it with the entity data.
        return new NewsArticleDTO
        {
            Id = input.Id.Value,
            Title = input.Title,
            Body = input.Body,
            DraftBody = input.DraftBody,
            DraftTitle = input.DraftTitle,
            Published = input.Published,
            Archived = input.Archived,
            Pinned = input.Pinned,
            CreatedDate = input.CreatedDate,
            ModifiedDate = input.ModifiedDate,
            DocumentType = 7
        };
    }
}
