using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandlers;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections.ChainOfResponsibility;

internal sealed class ManualSelectPupilsCommandHandler : ICommandHandler<UpdateMyPupilsSelectionStateRequest>
{
    public bool CanHandle(UpdateMyPupilsSelectionStateRequest input) => true;
    public void Handle(UpdateMyPupilsSelectionStateRequest input)
    {
        List<string> selectedPupilsOnPage = input.UpdateRequest.SelectedPupils?.ToList() ?? [];
        List<string> deselectedPupilsOnPage = input.UpdateRequest.CurrentPupils.Except(selectedPupilsOnPage).ToList();

        foreach (string upn in deselectedPupilsOnPage)
        {
            input.State.Deselect(upn);
        }
        foreach (string upn in selectedPupilsOnPage)
        {
            input.State.Select(upn);
        }
    }
}
