using DfE.GIAP.Web.Features.MyPupils.State.Models;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;
public record MyPupilsPresentationResponse
{
    public MyPupilsPresentationResponse(MyPupilsPresentationPupilModels pupils, MyPupilsState state)
    {
        MyPupils = pupils ??
            MyPupilsPresentationPupilModels.Create([]);

        ArgumentNullException.ThrowIfNull(state);

        PageNumber = state.PresentationState.Page;

        SortedDirection =
            state.PresentationState.SortDirection
                == SortDirection.Ascending
                    ? "asc" : "desc";

        SortedField = state.PresentationState.SortBy;

        IsAnyPupilsSelected = state.SelectionState.IsAnyPupilSelected;
    }

    public MyPupilsPresentationPupilModels MyPupils { get; }
    public int PageNumber { get; init; }
    public string SortedDirection { get; init; }
    public string SortedField { get; init; }
    public bool IsAnyPupilsSelected { get; init; }

    public static MyPupilsPresentationResponse Create(
        MyPupilsPresentationPupilModels pupils,
        MyPupilsState state) => new(pupils, state);
}
