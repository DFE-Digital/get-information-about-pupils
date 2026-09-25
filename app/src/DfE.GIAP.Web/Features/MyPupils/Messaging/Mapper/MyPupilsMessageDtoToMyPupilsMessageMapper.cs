using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.Messaging.Mapper;

public sealed class MyPupilsMessageDtoToMyPupilsMessageMapper : IMapper<MyPupilsMessageDto, MyPupilsMessage>
{
    public MyPupilsMessage Map(MyPupilsMessageDto input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new(
            id: input.Id,
            level: input.MessageLevel,
            message: input.Message);
    }
}
