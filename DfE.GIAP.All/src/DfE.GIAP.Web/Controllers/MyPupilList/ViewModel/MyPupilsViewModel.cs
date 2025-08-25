using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Response;

namespace DfE.GIAP.Web.Controllers.MyPupilList.ViewModel;

public sealed class MyPupilsViewModel
{
    private const int DEFAULT_PAGE_SIZE = 20;
    private readonly MyPupilsErrorModel? _error;

    public MyPupilsViewModel(
        IEnumerable<PupilPresentatationViewModel> pupils,
        MyPupilsErrorModel error = null)
    {
        Pupils = pupils;
        _error = error;
    }

    public bool isRemovePupilsSuccessful { get; set; } = false;
    public IEnumerable<PupilPresentatationViewModel> Pupils { get; }
    public bool SelectAll { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public bool IsAnyPupilsSelected { get; set; }
    public string SortField { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;
    public string PageHeading => "My pupil list";
    public string FormAction => "MyPupilList";
    public string UniquePupilNumberLabel => "UPN";

    public bool DisplayPreviousPage => PageNumber > 2; // If we enable this for Page 2, it will show 1, 1, 2
    public bool IsMorePageAvailable => Pupils.Count() == DEFAULT_PAGE_SIZE;
    public bool IsNoPupilsRemaining => !Pupils.Any(); 
    public string ErrorMessage => _error?.Message ?? string.Empty;
    public bool IsError => _error is not null;
}
