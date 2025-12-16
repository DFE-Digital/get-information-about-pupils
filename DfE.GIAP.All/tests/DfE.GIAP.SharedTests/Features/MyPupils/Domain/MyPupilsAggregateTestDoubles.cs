using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.SharedTests.Features.MyPupils.Domain;
public static class MyPupilsAggregateTestDoubles
{
    private const int DEFAULT_LIMIT = 4000;

    public static MyPupilsAggregate Default()
        => Create(
            MyPupilsIdTestDoubles.Default(),
            UniquePupilNumbers.Create(uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 10)),
            DEFAULT_LIMIT);

    public static MyPupilsAggregate Create(MyPupilsId id)
        => Create(
            id,
            UniquePupilNumbers.Create([]),
            DEFAULT_LIMIT);

    public static MyPupilsAggregate Create(UniquePupilNumbers uniquePupilNumbers)
    => Create(
            MyPupilsIdTestDoubles.Default(),
            uniquePupilNumbers);

    public static MyPupilsAggregate Create(MyPupilsId id, UniquePupilNumbers uniquePupilNumbers)
        => Create(
            id,
            uniquePupilNumbers,
            DEFAULT_LIMIT);

    private static MyPupilsAggregate Create(MyPupilsId id, UniquePupilNumbers uniquePupilNumbers, int limit)
        => new(
            id,
            uniquePupilNumbers,
            limit);
}
