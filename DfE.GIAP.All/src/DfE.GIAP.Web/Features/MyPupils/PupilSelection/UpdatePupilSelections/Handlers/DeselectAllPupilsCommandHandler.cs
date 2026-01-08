using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandlers;
using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections.Handlers;

internal sealed class DeselectAllPupilsCommandHandler : ICommandHandler<UpdateMyPupilsSelectionStateRequest>
{
    public bool CanHandle(UpdateMyPupilsSelectionStateRequest input)
        => input.UpdateRequest.SelectAllState == MyPupilsFormSelectionModeRequestDto.DeselectAll;

    public void Handle(UpdateMyPupilsSelectionStateRequest input)
    {
        ArgumentNullException.ThrowIfNull(input);
        input.State.DeselectAll();
    }
}
