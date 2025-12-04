using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.SharedTests.TestDoubles.MyPupils;
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
                MyPupilsIdTestDoubles.Default(),
                UniquePupilNumbers.Create(
                    uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 10))
            );
    }
}
