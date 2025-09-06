using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;

public record MyPupilsDtoSelectionStateDecorator
{
    public MyPupilsDtoSelectionStateDecorator(MyPupilDtos myPupilDtos, MyPupilsPupilSelectionState selectionState)
    {
        ArgumentNullException.ThrowIfNull(myPupilDtos);
        PupilDtos = myPupilDtos;

        ArgumentNullException.ThrowIfNull(selectionState);
        SelectionState = selectionState;
    }

    public MyPupilDtos PupilDtos { get; }
    public MyPupilsPupilSelectionState SelectionState { get; }
}
