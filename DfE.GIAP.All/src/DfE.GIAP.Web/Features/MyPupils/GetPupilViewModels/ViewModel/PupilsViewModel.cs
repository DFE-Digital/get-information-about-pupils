namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.ViewModel;

public record PupilsViewModel
{
    private readonly IEnumerable<PupilViewModel> _pupils;

    public PupilsViewModel(IEnumerable<PupilViewModel> Pupils)
    {
        _pupils = Pupils ?? [];
    }

    public IReadOnlyList<PupilViewModel> Pupils => _pupils.ToList().AsReadOnly();

    public int Count => _pupils.Count();

    public static PupilsViewModel Create(IEnumerable<PupilViewModel> pupils) => new(pupils);
}
