using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Application.Models;

namespace DfE.GIAP.SharedTests.TestDoubles.MyPupils;
public static class MyPupilsDocumentDtoTestDoubles
{
    public static MyPupilsDocumentDto Create(MyPupilsId id, UniquePupilNumbers upns)
    {
        List<MyPupilsPupilItemDto> pupilUpns =
            upns.GetUniquePupilNumbers()
                .Select((upn) => new MyPupilsPupilItemDto()
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
