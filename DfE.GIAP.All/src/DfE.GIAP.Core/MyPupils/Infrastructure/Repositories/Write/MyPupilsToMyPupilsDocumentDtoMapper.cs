using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.AggregateRoot;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write;
internal sealed class MyPupilsToMyPupilsDocumentDtoMapper : IMapper<Domain.AggregateRoot.MyPupils, MyPupilsDocumentDto>
{
    public MyPupilsDocumentDto Map(Domain.AggregateRoot.MyPupils input)
    {
        ArgumentNullException.ThrowIfNull(input);

        IEnumerable<MyPupilsPupilItemDto> updatedPupils = input.GetMyPupils()?.Select((upn) => new MyPupilsPupilItemDto()
        {
            UPN = upn.Value
        }) ?? [];

        MyPupilsDto pupilsDto = new()
        {
            Pupils = updatedPupils,
        };

        MyPupilsDocumentDto documentDto = new()
        {
            id = input.AggregateId.Value,
            MyPupils = pupilsDto
        };

        return documentDto;
    }
}
