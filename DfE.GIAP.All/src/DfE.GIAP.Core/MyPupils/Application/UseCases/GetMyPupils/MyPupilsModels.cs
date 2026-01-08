namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
public sealed class MyPupilsModels
{
    public MyPupilsModels(IEnumerable<MyPupilsModel> pupilDtos)
    {
        Values =
            (pupilDtos is null ? [] : pupilDtos)
                .ToList()
                .AsReadOnly();
    }

    public static MyPupilsModels Empty() => Create(pupils: []);
    public static MyPupilsModels Create(IEnumerable<MyPupilsModel> pupils) => new(pupils);

    public IReadOnlyList<MyPupilsModel> Values { get; }
    public IReadOnlyList<string> Identifiers => Values.Select(t => t.UniquePupilNumber).ToList().AsReadOnly();
    public int Count => Values.Count;
    public bool IsEmpty() => !Values.Any();
}
