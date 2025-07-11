using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.Mappers;

/// <summary>
/// Provides functionality to map a <see cref="NewsArticle"/> entity to a <see cref="NewsArticleDto"/> data transfer
/// object.
/// </summary>
/// <remarks>This mapper is responsible for converting the properties of a <see cref="NewsArticle"/> entity into a
/// corresponding <see cref="NewsArticleDto"/> object. It ensures that all relevant fields are transferred, including
/// metadata such as creation and modification dates.</remarks>
internal class NewsArticleEntityToDtoMapper : IMapper<NewsArticle, NewsArticleDto>
{
    /// <summary>
    /// Maps a <see cref="NewsArticle"/> entity to a <see cref="NewsArticleDto"/> data transfer object.
    /// </summary>
    /// <remarks>This method performs a direct mapping of properties from the <see cref="NewsArticle"/> entity
    /// to the <see cref="NewsArticleDto"/> object. The <c>DocumentType</c> property of the resulting DTO is set to a
    /// constant value of 7.</remarks>
    /// <param name="input">The <see cref="NewsArticle"/> instance to map. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="NewsArticleDto"/> instance populated with the corresponding data from the <paramref name="input"/>
    /// entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is <see langword="null"/>.</exception>
    public NewsArticleDto Map(NewsArticle input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new NewsArticleDto()
        {
            id = input.Id.Value,
            Title = input.Title,
            Body = input.Body,
            DraftTitle = input.DraftTitle,
            DraftBody = input.DraftBody,
            Published = input.Published,
            Pinned = input.Pinned,
            CreatedDate = input.CreatedDate,
            ModifiedDate = input.ModifiedDate,
        };
    }
}
