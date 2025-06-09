using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Core.Content.Infrastructure.Repositories.Mapper;
internal class ContentDTOToContentMapper : IMapper<ContentDTO, Application.Model.Content>
{
    public Application.Model.Content Map(ContentDTO input)
    {
        return new()
        {
            Title = input.Title ?? string.Empty,
            Body = input.Body ?? string.Empty
        };
    }
}
