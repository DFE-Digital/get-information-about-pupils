using DfE.GIAP.Web.Features.MyPupils.GetPupilViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModel;

public record MyPupilsViewModel
{
    private const int DEFAULT_PAGE_SIZE = 20;

    public MyPupilsViewModel(GetPupilViewModels.MyPupilsPresentationModel pupils)
    {
        ArgumentNullException.ThrowIfNull(pupils);
        Pupils = pupils;
    }

    public bool IsDeleteSuccessful { get; init; } = false;
    public bool IsAnyPupilsSelected { get; init; } = false;
    public bool SelectAll { get; init; } = false;
    public int PageNumber { get; init; } = 1;
    public string SortField { get; init; } = string.Empty;
    public string SortDirection { get; init; } = string.Empty;
    public MyPupilsErrorViewModel Error { get; init; } = MyPupilsErrorViewModel.NOOP();
    public MyPupilsPresentationModel Pupils { get; init; }
    public bool HasPupils => Pupils.Count > 0;
    public string PageHeading => "My pupil list";
    public string DownloadController => "DownloadMyPupils";
    public string DeleteMyPupilsController => "DeleteMyPupils";
    public string UpdateFormController => "UpdateMyPupilsForm";
    public string UniquePupilNumberLabel => "UPN";
    public bool DisplayPreviousPageNumber => PageNumber > 2; // If we enable this for Page 2, it will show 1, 1, 2
    public bool IsMorePageAvailable => Pupils.Count == DEFAULT_PAGE_SIZE;
}
