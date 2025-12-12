using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;
public record MyPupilsPresentationResponse
{
    public MyPupilsPresentationResponse(
        MyPupilsPresentationPupilModels currentPupils,
        MyPupilsPresentationQueryModel presentation,
        MyPupilsPupilSelectionState selectionState,
        int totalPupilCount)
    {
        MyPupils = currentPupils ??
            MyPupilsPresentationPupilModels.Create([]);

        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(presentation);

        PageNumber = presentation.Page.Value;

        SortedDirection = presentation.Sort.Direction switch
        {
            SortDirection.Ascending => "asc",
            SortDirection.Descending => "desc",
            _ => string.Empty
        };

        SortedField = presentation.Sort.Field;

        IsAnyPupilsSelected = selectionState.IsAnyPupilSelected;

        TotalPages = totalPupilCount == 0 ? 1
            : (int)Math.Ceiling(totalPupilCount / (double)presentation.PageSize);
    }

    public MyPupilsPresentationPupilModels MyPupils { get; }
    public int PageNumber { get; init; }
    public int TotalPages { get; init; }
    public string SortedDirection { get; init; }
    public string SortedField { get; init; }
    public bool IsAnyPupilsSelected { get; init; }
}
