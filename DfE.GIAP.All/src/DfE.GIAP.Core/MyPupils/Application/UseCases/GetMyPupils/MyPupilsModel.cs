using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
public sealed class MyPupilsModel
{
    public MyPupilsModel(IEnumerable<MyPupilModel> pupilDtos)
    {
        Values =
            (pupilDtos is null ? [] : pupilDtos)
                .ToList()
                .AsReadOnly();
    }

    public static MyPupilsModel Empty() => Create([]);
    public static MyPupilsModel Create(IEnumerable<MyPupilModel> pupils) => new(pupils);

    public IReadOnlyList<MyPupilModel> Values { get; }
    public IReadOnlyList<string> Identifiers => Values.Select(t => t.UniquePupilNumber).ToList().AsReadOnly();
    public int Count => Values.Count;
    public bool IsEmpty() => !Values.Any();
}
