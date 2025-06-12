using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Core.Content.Infrastructure.Repositories.Mapper;

/// <summary>
/// Maps a <see cref="ContentDto"/> object to a domain <see cref="Application.Model.Content"/> object.
/// </summary>
internal class ContentDtoToContentMapper : IMapper<ContentDto?, Application.Model.Content>
{
    /// <summary>
    /// Maps the specified <see cref="ContentDto"/> to a <see cref="Application.Model.Content"/>.
    /// Returns an empty content object if the input is null.
    /// </summary>
    /// <param name="input">The DTO to map from.</param>
    /// <returns>A mapped <see cref="Application.Model.Content"/> instance.</returns>
    public Application.Model.Content Map(ContentDto? input)
    {
        if (input == null)
        {
            return Application.Model.Content.Empty();
        }

        return new()
        {
            Title = input.Title ?? string.Empty,
            Body = input.Body ?? string.Empty
        };
    }
}
