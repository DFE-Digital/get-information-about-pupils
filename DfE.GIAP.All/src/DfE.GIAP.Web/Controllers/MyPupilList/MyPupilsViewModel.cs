namespace DfE.GIAP.Web.Controllers.MyPupilList;

public sealed class MyPupilsViewModel
{
    private readonly MyPupilsErrorModel? _error;
    public MyPupilsViewModel(
        IEnumerable<PupilPresentatationModel> pupils,
        MyPupilsErrorModel? error = null)
    {
        Pupils = pupils;
        _error = error;
    }

    public string PageHeading => "My pupil list";
    public string FormAction => "MyPupilList";
    public string UniquePupilNumberLabel => "UPN";
    public IEnumerable<PupilPresentatationModel> Pupils { get; } = [];
    public IEnumerable<string> SelectedPupils { get; } = [];
    public bool IsAnyPupilsSelected => SelectAll || SelectedPupils.Any();
    public bool SelectAll { get; set; } = false;

    public bool IsPreviousPageAvailable => PageNumber > 1;
    public bool IsMorePageAvailable => Pupils.Count() == PageSize;
    public bool NoPupils => Pupils.Count() == 0;

    public int PageNumber { get; set; } = 1;
    public int PageSize => 20;
    public string SortField { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;


    public bool isRemovedSuccessful => false;

    public string ErrorMessage => _error?.Message ?? string.Empty;
    public bool IsError => _error is not null;
}
