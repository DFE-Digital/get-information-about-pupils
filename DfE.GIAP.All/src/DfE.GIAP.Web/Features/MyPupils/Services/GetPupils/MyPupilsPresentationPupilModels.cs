namespace DfE.GIAP.Web.Features.MyPupils.Services.GetPupils;

public sealed record MyPupilsPresentationPupilModels
{
    private readonly IReadOnlyList<MyPupilsPresentationPupilModel> _pupils;

    public MyPupilsPresentationPupilModels(IEnumerable<MyPupilsPresentationPupilModel> pupils)
    {
        _pupils = (pupils ?? []).ToList().AsReadOnly();
    }

    public IReadOnlyList<MyPupilsPresentationPupilModel> Values => _pupils;

    public int Count => _pupils.Count;

    public static MyPupilsPresentationPupilModels Create(IEnumerable<MyPupilsPresentationPupilModel> pupils) => new(pupils);
    public static MyPupilsPresentationPupilModels Empty() => Create(pupils: []);
}
