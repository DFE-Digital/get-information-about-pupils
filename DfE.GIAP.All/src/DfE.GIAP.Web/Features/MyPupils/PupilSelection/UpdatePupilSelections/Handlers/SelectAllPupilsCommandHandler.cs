using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v1;
using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections.Handlers;

internal sealed class SelectAllPupilsCommandHandler : IEvaluationHandler<UpdateMyPupilsSelectionStateRequest>
{
    public bool CanHandle(UpdateMyPupilsSelectionStateRequest input)
        => input.UpdateRequest.SelectAllState == MyPupilsFormSelectionModeRequestDto.SelectAll;

    public void Handle(UpdateMyPupilsSelectionStateRequest input)
    {
        ArgumentNullException.ThrowIfNull(input);
        input.State.SelectAll();
    }
}
