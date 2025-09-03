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
    public int Count => Values.Count;
    public bool IsEmpty() => !Values.Any();
}
