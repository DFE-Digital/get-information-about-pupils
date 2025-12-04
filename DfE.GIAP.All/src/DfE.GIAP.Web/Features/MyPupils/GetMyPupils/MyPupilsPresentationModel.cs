namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupils;

public record MyPupilsPresentationModel
{
    private readonly IEnumerable<MyPupilsPupilPresentationModel> _pupils;

    public MyPupilsPresentationModel(IEnumerable<MyPupilsPupilPresentationModel> Pupils)
    {
        _pupils = Pupils ?? [];
    }

    public IReadOnlyList<MyPupilsPupilPresentationModel> Pupils => _pupils.ToList().AsReadOnly();

    public int Count => _pupils.Count();

    public static MyPupilsPresentationModel Create(IEnumerable<MyPupilsPupilPresentationModel> pupils) => new(pupils);
}
