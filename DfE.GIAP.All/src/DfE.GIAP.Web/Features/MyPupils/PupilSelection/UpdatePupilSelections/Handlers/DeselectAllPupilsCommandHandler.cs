using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections.Handlers;

internal sealed class DeselectAllPupilsCommandHandler : IEvaluationHandler<UpdateMyPupilsSelectionStateRequest>
{
    public ValueTask<HandlerResult> HandleAsync(UpdateMyPupilsSelectionStateRequest input, CancellationToken ctx = default)
    {
        if (input is null)
        {
            return HandlerResultValueTaskFactory.FailedWithNullArgument(nameof(input));
        }

        if (input.UpdateRequest.SelectAllState != MyPupilsFormSelectionModeRequestDto.DeselectAll)
        {
            return HandlerResultValueTaskFactory.Skipped();
        }

        input.State.DeselectAll();
        return HandlerResultValueTaskFactory.SuccessfulResult();
    }
}
