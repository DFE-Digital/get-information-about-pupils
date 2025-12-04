using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupils.Mapper;

public record PupilsSelectionContext
{
    public PupilsSelectionContext(MyPupilsModel myPupils, MyPupilsPupilSelectionState selectionState)
    {
        ArgumentNullException.ThrowIfNull(myPupils);
        Pupils = myPupils;

        ArgumentNullException.ThrowIfNull(selectionState);
        SelectionState = selectionState;
    }

    public MyPupilsModel Pupils { get; }
    public MyPupilsPupilSelectionState SelectionState { get; }
}
