using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.Messaging.Mapper;

public sealed class MyPupilsMessageToMyPupilsMessageDtoMapper : IMapper<MyPupilsMessage, MyPupilsMessageDto>
{
    public MyPupilsMessageDto Map(MyPupilsMessage input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new()
        {
            Id = input.Id,
            Message = input.Message,
            MessageLevel = input.Level
        };
    }
}
