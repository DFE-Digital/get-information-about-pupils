using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.SharedTests.TestDoubles.MyPupils;
public static class MyPupilsTestDoubles
{
    private const int DEFAULT_LIMIT = 4000;

    public static Core.MyPupils.Domain.AggregateRoot.MyPupils Default()
        => Create(
            UserIdTestDoubles.Default(),
            UniquePupilNumbers.Create(
                uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 10)),
            DEFAULT_LIMIT);

    public static Core.MyPupils.Domain.AggregateRoot.MyPupils Create(UniquePupilNumbers uniquePupilNumbers)
        => Create(
            UserIdTestDoubles.Default(),
            uniquePupilNumbers);

    public static Core.MyPupils.Domain.AggregateRoot.MyPupils Create(UserId userId, UniquePupilNumbers uniquePupilNumbers)
        => Create(userId, uniquePupilNumbers, DEFAULT_LIMIT);

    private static Core.MyPupils.Domain.AggregateRoot.MyPupils Create(UserId userId, UniquePupilNumbers uniquePupilNumbers, int limit)
        => new(
            userId,
            uniquePupilNumbers,
            limit);
}
