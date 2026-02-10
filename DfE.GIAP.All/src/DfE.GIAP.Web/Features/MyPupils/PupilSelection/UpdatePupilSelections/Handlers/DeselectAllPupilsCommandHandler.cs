using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections.Handlers;

internal sealed class DeselectAllPupilsCommandHandler : IEvaluationHandler<UpdateMyPupilsSelectionStateRequest>
{
    public ValueTask<HandlerResult> HandleAsync(UpdateMyPupilsSelectionStateRequest input, CancellationToken ctx = default)
    {
        if (input is null)
        {
            return HandlerResultValueTaskFactory.FailedWithNullArgument(nameof(input));
        }

        if (input.UpdateRequest.SelectAllState != MyPupilsPupilSelectionModeRequestDto.DeselectAll)
        {
            return HandlerResultValueTaskFactory.Skipped();
        }

        input.State.DeselectAll();
        return HandlerResultValueTaskFactory.SuccessfulResult();
    }
}
