using DfE.GIAP.Web.Controllers.MyPupilList.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers.MyPupilList;

public sealed class MyPupilsFormStateRequestDto
{
    [FromForm]
    public bool? SelectAll { get; set; } = null; // Should all pupils be selected

    [FromForm]
    public List<string> SelectedPupils { get; set; } = []; // Selected pupils on current page

    [FromForm]
    public string CurrentPageOfPupils { get; set; } = string.Empty;

    [FromQuery]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public string SortField { get; set; } = string.Empty;

    [FromQuery]
    public string SortDirection { get; set; } = string.Empty;
    public MyPupilsErrorModel Error { get; set; } = null; // Used by other actions to error when posting back to the form
}
