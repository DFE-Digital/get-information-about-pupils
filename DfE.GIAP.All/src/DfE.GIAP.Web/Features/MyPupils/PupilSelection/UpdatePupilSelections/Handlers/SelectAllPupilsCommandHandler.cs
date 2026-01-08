using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandlers;
using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections.Handlers;

internal sealed class SelectAllPupilsCommandHandler : ICommandHandler<UpdateMyPupilsSelectionStateRequest>
{
    public bool CanHandle(UpdateMyPupilsSelectionStateRequest input)
        => input.UpdateRequest.SelectAllState == MyPupilsFormSelectionModeRequestDto.SelectAll;

    public void Handle(UpdateMyPupilsSelectionStateRequest input)
    {
        ArgumentNullException.ThrowIfNull(input);
        input.State.SelectAll();
    }
}
