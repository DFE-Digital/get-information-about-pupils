using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.Mapper;

public record MyPupilsModelSelectionStateDecorator
{
    public MyPupilsModelSelectionStateDecorator(MyPupilsModel myPupils, MyPupilsPupilSelectionState selectionState)
    {
        ArgumentNullException.ThrowIfNull(myPupils);
        Pupils = myPupils;

        ArgumentNullException.ThrowIfNull(selectionState);
        SelectionState = selectionState;
    }

    public MyPupilsModel Pupils { get; }
    public MyPupilsPupilSelectionState SelectionState { get; }
}
