namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections.Handlers;

internal sealed class ManualSelectPupilsCommandHandler : IEvaluationHandler<UpdateMyPupilsSelectionStateRequest>
{
    public ValueTask<HandlerResult> HandleAsync(UpdateMyPupilsSelectionStateRequest input, CancellationToken ctx = default)
    {
        if (input is null)
        {
            return HandlerResultValueTaskFactory.FailedWithNullArgument(nameof(input));
        }

        List<string> selectedPupilsOnPage = input.UpdateRequest.SelectedPupils?.ToList() ?? [];
        List<string> deselectedPupilsOnPage = input.UpdateRequest.CurrentPupils?.Except(selectedPupilsOnPage).ToList() ?? [];

        foreach (string upn in deselectedPupilsOnPage)
        {
            input.State.Deselect(upn);
        }

        foreach (string upn in selectedPupilsOnPage)
        {
            input.State.Select(upn);
        }

        return HandlerResultValueTaskFactory.SuccessfulResult();
    }
}
