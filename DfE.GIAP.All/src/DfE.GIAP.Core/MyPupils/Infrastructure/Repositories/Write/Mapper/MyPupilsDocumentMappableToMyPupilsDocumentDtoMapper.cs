using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write.Mapper;
internal sealed class MyPupilsDocumentMappableToMyPupilsDocumentDtoMapper : IMapper<MyPupilsDocumentDtoMappable, MyPupilsDocumentDto>
{
    public MyPupilsDocumentDto Map(MyPupilsDocumentDtoMappable input)
    {
        IEnumerable<MyPupilsPupilItemDto> updatedPupils = input.Upns.AsValues()?.Select((upn) => new MyPupilsPupilItemDto()
        {
            UPN = upn
        }) ?? [];

        MyPupilsDto pupilsDto = new()
        {
            Pupils = updatedPupils,
        };

        MyPupilsDocumentDto documentDto = new()
        {
            id = input.UserId.Value,
            MyPupils = pupilsDto
        };

        return documentDto;
    }
}
