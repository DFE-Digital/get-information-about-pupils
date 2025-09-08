using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.ViewModel;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModels;

public sealed class MyPupilsViewModel
{
    private const int DEFAULT_PAGE_SIZE = 20;

    public MyPupilsViewModel(
        PupilsViewModel pupils,
        MyPupilsErrorViewModel? error = null)
    {
        ArgumentNullException.ThrowIfNull(pupils);
        Pupils = pupils;
        Error = error ?? new(string.Empty);
    }

    public bool isRemovePupilsSuccessful { get; set; } = false;
    public PupilsViewModel Pupils { get; }
    public MyPupilsErrorViewModel Error { get; }
    public bool HasPupils => Pupils.Count > 0;
    public bool IsError => !string.IsNullOrEmpty(Error.Message);
    public bool SelectAll { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public bool IsAnyPupilsSelected { get; set; }
    public string SortField { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;
    public string PageHeading => "My pupil list";
    public string FormAction => "MyPupilList";
    public string UniquePupilNumberLabel => "UPN";
    public bool DisplayPreviousPage => PageNumber > 2; // If we enable this for Page 2, it will show 1, 1, 2
    public bool IsMorePageAvailable => Pupils.Count == DEFAULT_PAGE_SIZE;
    public bool IsNoPupilsRemaining => Pupils.Count == 0;
}
