using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState.PupilSelectionStateUpdater;

public interface IPupilSelectionStateUpdateHandler
{
    void Handle(MyPupilsPupilSelectionState state,
        IEnumerable<UniquePupilNumber> currentPageOfPupils,
        MyPupilsFormStateRequestDto updateState);
}
