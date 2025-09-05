using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
public static class MyPupilsDocumentDtoTestDoubles
{
    public static MyPupilsDocumentDto Create(UserId userId, UniquePupilNumbers upns)
    {
        List<MyPupilsPupilItemDto> pupilUpns =
            upns.AsValues()
                .Select((upn) => new MyPupilsPupilItemDto() { UPN = upn })
                .ToList();
        return new()
        {
            id = userId.Value,
            MyPupils = new()
            {
                Pupils = pupilUpns
            }
        };
    }

    public static MyPupilsDocumentDto Default()
    {
        return Create(
                UserIdTestDoubles.Default(),
                UniquePupilNumbers.Create(
                    uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 10))
            );
    }
}
