using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Core.Content.Infrastructure.Repositories.Mapper;
internal class ContentDtoToContentMapper : IMapper<ContentDto?, Application.Model.Content>
{
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
