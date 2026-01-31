using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.Features.MyPupils.Domain;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.SharedTests.Features.MyPupils.DataTransferObjects;
public static class MyPupilsDocumentDtoTestDoubles
{
    public static MyPupilsDocumentDto Create(MyPupilsId id, UniquePupilNumbers upns)
    {
        List<MyPupilsPupilDto> pupilUpns =
            upns.GetUniquePupilNumbers()
                .Select((upn) => new MyPupilsPupilDto()
                {
                    UPN = upn.Value
                })
                .ToList();

        return new()
        {
            id = id.Value,
            MyPupils = new()
            {
                Pupils = pupilUpns
            }
        };
    }

    public static MyPupilsDocumentDto Default()
    {
        return Create(
                id: MyPupilsIdTestDoubles.Default(),
                upns: UniquePupilNumbers.Create(
                    uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 10))
            );
    }
}
