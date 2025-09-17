using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles.MyPupils;
public static class MyPupilsAggregateRootTestDoubles
{
    private const int DEFAULT_LIMIT = 4000;

    public static Core.MyPupils.Domain.AggregateRoot.MyPupils Default()
        => Create(
            MyPupilsIdTestDoubles.Default(),
            UniquePupilNumbers.Create(uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 10)),
            DEFAULT_LIMIT);

    public static Core.MyPupils.Domain.AggregateRoot.MyPupils Create(MyPupilsId id)
        => Create(
            id,
            UniquePupilNumbers.Create([]),
            DEFAULT_LIMIT);

    public static Core.MyPupils.Domain.AggregateRoot.MyPupils Create(UniquePupilNumbers uniquePupilNumbers)
    => Create(
            MyPupilsIdTestDoubles.Default(),
            uniquePupilNumbers);

    public static Core.MyPupils.Domain.AggregateRoot.MyPupils Create(MyPupilsId id, UniquePupilNumbers uniquePupilNumbers)
        => Create(
            id,
            uniquePupilNumbers,
            DEFAULT_LIMIT);

    private static Core.MyPupils.Domain.AggregateRoot.MyPupils Create(MyPupilsId id, UniquePupilNumbers uniquePupilNumbers, int limit)
        => new(
            id,
            uniquePupilNumbers,
            limit);
}
