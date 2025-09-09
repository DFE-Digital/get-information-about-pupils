using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModel;

public sealed class MyPupilsViewModel
{
    private const int DEFAULT_PAGE_SIZE = 20;

    public MyPupilsViewModel(
        PupilsViewModel pupils,
        MyPupilsErrorViewModel error = null)
    {
        ArgumentNullException.ThrowIfNull(pupils);
        Pupils = pupils;
        Error = error ?? new(string.Empty);
    }

    public bool isRemovePupilsSuccessful { get; set; } = false;
    public bool IsAnyPupilsSelected { get; set; }
    public bool SelectAll { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public string SortField { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;
    public PupilsViewModel Pupils { get; }
    public MyPupilsErrorViewModel Error { get; }
    public bool HasPupils => Pupils.Count > 0;
    public bool IsError => !string.IsNullOrEmpty(Error.Message);
    public string PageHeading => "My pupil list";
    public string DownloadController => "MyPupilsDownload";
    public string DeleteMyPupilsController => "MyPupilsDeletePupils";
    public string DeleteMyPupilsAction => "DeletePupils";
    public string UpdateFormController => "MyPupilsUpdateForm";
    public string UniquePupilNumberLabel => "UPN";
    public bool DisplayPreviousPageNumber => PageNumber > 2; // If we enable this for Page 2, it will show 1, 1, 2
    public bool IsMorePageAvailable => Pupils.Count == DEFAULT_PAGE_SIZE;
    public bool IsNoPupilsRemaining => Pupils.Count == 0;
}
