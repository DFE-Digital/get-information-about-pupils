using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModel;

public record MyPupilsViewModel
{
    private const int DEFAULT_PAGE_SIZE = 20;

    public MyPupilsViewModel(PupilsViewModel pupils)
    {
        ArgumentNullException.ThrowIfNull(pupils);
    }

    public bool IsDeleteSuccessful { get; init; } = false;
    public bool IsAnyPupilsSelected { get; init; }
    public bool SelectAll { get; init; } = false;
    public int PageNumber { get; init; } = 1;
    public string SortField { get; init; } = string.Empty;
    public string SortDirection { get; init; } = string.Empty;
    public PupilsViewModel Pupils { get; }
    public MyPupilsErrorViewModel Error { get; init; }
    public bool HasPupils => Pupils.Count > 0;
    public bool IsError => !string.IsNullOrEmpty(Error.Message);
    public string PageHeading => "My pupil list";
    public string DownloadController => "MyPupilsDownload";
    public string DeleteMyPupilsController => "MyPupilsDeletePupils";
    public string UpdateFormController => "MyPupilsUpdateForm";
    public string UniquePupilNumberLabel => "UPN";
    public bool DisplayPreviousPageNumber => PageNumber > 2; // If we enable this for Page 2, it will show 1, 1, 2
    public bool IsMorePageAvailable => Pupils.Count == DEFAULT_PAGE_SIZE;
}
