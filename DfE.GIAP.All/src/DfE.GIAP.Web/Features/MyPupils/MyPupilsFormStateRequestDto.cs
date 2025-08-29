using DfE.GIAP.Web.Features.MyPupils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils;

public sealed class MyPupilsFormStateRequestDto
{
    // TODO consider CustomModelBinding to SelectAllState, which is what should be used in Controller to pass down
    [FromForm]
    public bool? SelectAll { get; set; } = null; // Should all pupils be selected

    [FromForm]
    // TODO Validator for ModelState, Validate XSS
    public List<string> SelectedPupils { get; set; } = []; // Selected pupils on current page

    [FromQuery]
    // TODO Validator for ModelState
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    // TODO Validator for ModelState, Validate XSS
    public string SortField { get; set; } = string.Empty;

    [FromQuery]
    // TODO Validator for ModelState, Validate XSS
    public string SortDirection { get; set; } = string.Empty;
    public MyPupilsErrorViewModel Error { get; set; } = null; // Used by other actions to error when posting back to the form

    public MyPupilsFormSelectAllStateRequestDto SelectAllState
        => SelectAll.HasValue && SelectAll.Value ? MyPupilsFormSelectAllStateRequestDto.SelectAll :
                SelectAll.HasValue && !SelectAll.Value ? MyPupilsFormSelectAllStateRequestDto.DeselectAll :
                    MyPupilsFormSelectAllStateRequestDto.NotSpecified;
}
