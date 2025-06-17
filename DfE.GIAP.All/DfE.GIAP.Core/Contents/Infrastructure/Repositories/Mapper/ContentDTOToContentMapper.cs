using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.Models;

namespace DfE.GIAP.Core.Contents.Infrastructure.Repositories.Mapper;

/// <summary>
/// Maps a <see cref="ContentDto"/> object to a domain <see cref="Content"/> object.
/// </summary>
internal class ContentDtoToContentMapper : IMapper<ContentDto?, Content>
{
    /// <summary>
    /// Maps the specified <see cref="ContentDto"/> to a <see cref="Content"/>.
    /// Returns an empty content object if the input is null.
    /// </summary>
    /// <param name="input">The DTO to map from.</param>
    /// <returns>A mapped <see cref="Content"/> instance.</returns>
    public Content Map(ContentDto? input)
    {
        if (input == null)
        {
            return Content.Empty();
        }

        return new()
        {
            Title = input.Title ?? string.Empty,
            Body = input.Body ?? string.Empty
        };
    }
}
