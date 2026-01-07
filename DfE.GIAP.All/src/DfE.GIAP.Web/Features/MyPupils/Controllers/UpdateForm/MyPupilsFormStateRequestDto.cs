using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

public record MyPupilsFormStateRequestDto
{
    // TODO consider CustomModelBinding to SelectAllState, which is what should be used in Controller to pass down OR Mapper
    [FromForm]
    public bool? SelectAll { get; set; } = null; // Should all pupils be selected

    [FromForm]
    // TODO Validator for ModelState, Validate XSS, PageNumber fixed size guard
    public List<string> SelectedPupils { get; set; } = []; // Selected pupils on current page

    // TODO Validator for ModelState, Validate XSS, PageNumber fixed size guard
    [FromForm]
    public List<string> CurrentPupils { get; set; } = [];

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
