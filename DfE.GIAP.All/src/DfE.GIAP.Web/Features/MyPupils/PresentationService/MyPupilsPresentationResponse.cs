using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;
public record MyPupilsPresentationResponse
{
    public MyPupilsPresentationResponse(
        MyPupilsPresentationPupilModels pupils,
        MyPupilsPresentationQueryModel presentation,
        MyPupilsPupilSelectionState selectionState)
    {
        MyPupils = pupils ??
            MyPupilsPresentationPupilModels.Create([]);

        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(presentation);

        PageNumber = presentation.Page.Value;

        SortedDirection =
            presentation.Sort.Direction == SortDirection.Ascending ? "asc" : "desc";

        SortedField = presentation.Sort.Field;

        IsAnyPupilsSelected = selectionState.IsAnyPupilSelected;
    }

    public MyPupilsPresentationPupilModels MyPupils { get; }
    public int PageNumber { get; init; }
    public string SortedDirection { get; init; }
    public string SortedField { get; init; }
    public bool IsAnyPupilsSelected { get; init; }

    public static MyPupilsPresentationResponse Create(
        MyPupilsPresentationPupilModels pupils,
        MyPupilsPresentationQueryModel presentation,
        MyPupilsPupilSelectionState selectionState) => new(pupils, presentation, selectionState);
}
