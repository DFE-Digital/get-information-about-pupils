using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.Mapper;

public record MyPupilsDtoSelectionStateDecorator
{
    public MyPupilsDtoSelectionStateDecorator(MyPupilsModel myPupilDtos, MyPupilsPupilSelectionState selectionState)
    {
        ArgumentNullException.ThrowIfNull(myPupilDtos);
        PupilDtos = myPupilDtos;

        ArgumentNullException.ThrowIfNull(selectionState);
        SelectionState = selectionState;
    }

    public MyPupilsModel PupilDtos { get; }
    public MyPupilsPupilSelectionState SelectionState { get; }
}
