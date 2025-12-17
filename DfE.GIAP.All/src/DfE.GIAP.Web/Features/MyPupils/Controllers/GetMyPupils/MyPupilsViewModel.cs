using DfE.GIAP.Web.Features.MyPupils.PresentationService;

namespace DfE.GIAP.Web.Features.MyPupils.Areas.GetMyPupils;

public record MyPupilsViewModel
{
    private const int DEFAULT_PAGE_SIZE = 20;
    public bool IsDeleteSuccessful { get; init; } = false;
    public bool IsAnyPupilsSelected { get; init; } = false;
    public string Error { get; init; } = string.Empty;
    public int PageNumber { get; init; } = 1;
    public int LastPageNumber { get; init; } = 1;
    public string SortField { get; init; } = string.Empty;
    public string SortDirection { get; init; } = string.Empty;
    public MyPupilsPresentationPupilModels CurrentPageOfPupils { get; init; }
    public bool HasPupils => CurrentPageOfPupils.Count > 0;
    public bool MorePagesAvailable =>
        CurrentPageOfPupils.Count == DEFAULT_PAGE_SIZE &&
            LastPageNumber > PageNumber;

    public string PageHeading => "My pupil list";
    public string DownloadController => "DownloadMyPupils";
    public string DeleteMyPupilsController => "DeleteMyPupils";
    public string UpdateFormController => "UpdateMyPupilsForm";
    public string UpdateFormAction => "Index";
    public string UniquePupilNumberLabel => "UPN";
}
