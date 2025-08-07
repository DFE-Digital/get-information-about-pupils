using DfE.GIAP.Web.Controllers.MyPupilList.FormState;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DfE.GIAP.Web.Controllers.MyPupilList.ViewModel;

public sealed class MyPupilsViewModel
{
    private const int DEFAULT_PAGE_SIZE = 20;
    private readonly ITempDataDictionary _tempData; // TEMP while View is shared
    private readonly MyPupilsFormState _formState;
    private MyPupilsErrorModel _error;

    public MyPupilsViewModel(
        IEnumerable<PupilPresentatationModel> pupils,
        MyPupilsFormState formState,
        MyPupilsErrorModel error = null,
        ITempDataDictionary tempData = null)
    {
        Pupils = pupils;
        _formState = formState;
        _error = error;
        _tempData = tempData;
    }

    public IEnumerable<PupilPresentatationModel> Pupils { get; }
    public IEnumerable<string> SelectedPupils => _formState.SelectedPupils;
    public bool SelectAll => _formState.SelectAll;
    public int PageNumber => _formState.Page.Value;
    public string SortField => _formState.SortBy;
    public string SortDirection => _formState.SortDirection.ToFormState();
    public string PageHeading => "My pupil list";
    public string FormAction => "MyPupilList";
    public string UniquePupilNumberLabel => "UPN";

    public bool DisplayPreviousPage => PageNumber > 2; // If we enable this for Page 2, it will show 1, 1, 2
    public bool IsMorePageAvailable => Pupils.Count() == DEFAULT_PAGE_SIZE;
    public bool IsNoPupilsRemaining => !Pupils.Any();

    public bool isRemovePupilsSuccessful => _tempData?["IsRemoveSuccessful"] as bool? ?? false;

    public void SetError(MyPupilsErrorModel error) => _error = error;
    public string ErrorMessage => _error?.Message ?? string.Empty;
    public bool IsError => _error is not null;
}
