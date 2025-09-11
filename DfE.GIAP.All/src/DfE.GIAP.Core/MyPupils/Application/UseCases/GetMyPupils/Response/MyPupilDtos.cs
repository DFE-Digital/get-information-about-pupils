using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
public sealed class MyPupilDtos
{
    public MyPupilDtos(IEnumerable<MyPupilDto> pupilDtos)
    {
        Values =
            (pupilDtos is null ? [] : pupilDtos)
                .ToList()
                .AsReadOnly();
    }

    public static MyPupilDtos Empty() => Create([]);
    public static MyPupilDtos Create(IEnumerable<MyPupilDto> pupils) => new(pupils);

    public IReadOnlyList<MyPupilDto> Values { get; }
    public IReadOnlyList<UniquePupilNumber> Identifiers => Values.Select(t => t.UniquePupilNumber).ToList().AsReadOnly();
    public int Count => Values.Count;
    public bool IsEmpty() => !Values.Any();
}
