using DfE.GIAP.Web.Features.MyPupils.Routes.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;

public sealed class MyPupilsFormStateRequestDto
{
    // TODO consider CustomModelBinding to SelectAllState, which is what should be used in Controller to pass down OR Mapper
    [FromForm]
    public bool? SelectAll { get; set; } = null; // Should all pupils be selected

    [FromForm]
    // TODO Validator for ModelState, Validate XSS, PageNumber fixed size guard
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

    public MyPupilsFormSelectionModeRequestDto SelectAllState
    {
        get
        {
            if (SelectAll.HasValue && SelectAll.Value)
            {
                return MyPupilsFormSelectionModeRequestDto.SelectAll;
            }
            if (SelectAll.HasValue && !SelectAll.Value)
            {
                return MyPupilsFormSelectionModeRequestDto.DeselectAll;
            }
            return MyPupilsFormSelectionModeRequestDto.ManualSelection; ;
        }
    }
}
