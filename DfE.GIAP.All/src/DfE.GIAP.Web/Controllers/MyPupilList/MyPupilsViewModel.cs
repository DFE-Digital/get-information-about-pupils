namespace DfE.GIAP.Web.Controllers.MyPupilList;

public sealed class MyPupilsViewModel
{
    private readonly MyPupilsErrorModel? _error;
    private const int DEFAULT_PAGE_SIZE = 20;
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
    public IEnumerable<string> SelectedPupils { get; set; } = [];
    public bool IsAnyPupilsSelected => SelectAll || SelectedPupils.Any();
    public bool SelectAll { get; set; } = false;

    public bool DisplayPreviousPage => PageNumber > 2; // If we enable this for Page 2, it will show 1, 1, 2
    public bool IsMorePageAvailable => Pupils.Count() == DEFAULT_PAGE_SIZE;
    public bool IsNoPupilsRemaining => !Pupils.Any();

    public int PageNumber { get; set; } = 1;
    public string SortField { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;

    public bool isRemovedSuccessful => false;

    public string ErrorMessage => _error?.Message ?? string.Empty;
    public bool IsError => _error is not null;
}
