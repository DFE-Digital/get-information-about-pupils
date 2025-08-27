using DfE.GIAP.Web.Features.MyPupils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils;

public sealed class MyPupilsFormStateRequestDto
{
    [FromForm]
    public bool? SelectAll { get; set; } = null; // Should all pupils be selected

    [FromForm]
    // TODO Validator for ModelState, InCurrentPageOfPupils
    public List<string> SelectedPupils { get; set; } = []; // Selected pupils on current page

    //[FromForm]
    //// TODO Validator for ModelState
    //public string CurrentPageOfPupils { get; set; } = string.Empty;

    [FromQuery]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public string SortField { get; set; } = string.Empty;

    [FromQuery]
    public string SortDirection { get; set; } = string.Empty;
    public MyPupilsErrorViewModel Error { get; set; } = null; // Used by other actions to error when posting back to the form

    public SelectAllStateRequestDto SelectAllState
        => SelectAll.HasValue && SelectAll.Value ? SelectAllStateRequestDto.SelectAll :
                SelectAll.HasValue && !SelectAll.Value ? SelectAllStateRequestDto.DeselectAll :
                    SelectAllStateRequestDto.NotSpecified;
    //public IEnumerable<string> ParseCurrentPageOfPupils()
    //{
    //    return CurrentPageOfPupils
    //            .Split(',', StringSplitOptions.RemoveEmptyEntries)
    //            .Select(t => t.ReplaceLineEndings().Trim())
    //            .Where(UniquePupilNumberValidator.Validate);
    //}
}

public enum SelectAllStateRequestDto
{
    SelectAll,
    DeselectAll,
    NotSpecified
}
