namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
public sealed class MyPupilsModel
{
    public MyPupilsModel(IEnumerable<MyPupilDto> pupilDtos)
    {
        Values =
            (pupilDtos is null ? [] : pupilDtos)
                .ToList()
                .AsReadOnly();
    }

    public static MyPupilsModel Empty() => Create([]);
    public static MyPupilsModel Create(IEnumerable<MyPupilDto> pupils) => new(pupils);

    public IReadOnlyList<MyPupilDto> Values { get; }
    public IReadOnlyList<string> Identifiers => Values.Select(t => t.UniquePupilNumber).ToList().AsReadOnly();
    public int Count => Values.Count;
    public bool IsEmpty() => !Values.Any();
}
