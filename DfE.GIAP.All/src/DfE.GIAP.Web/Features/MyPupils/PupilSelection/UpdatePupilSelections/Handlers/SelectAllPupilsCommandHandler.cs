using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections.Handlers;

internal sealed class SelectAllPupilsCommandHandler : IEvaluationHandler<UpdateMyPupilsSelectionStateRequest>
{
    public ValueTask<HandlerResult> HandleAsync(UpdateMyPupilsSelectionStateRequest input, CancellationToken ctx = default)
    {
        if (input is null)
        {
            return HandlerResultValueTaskFactory.FailedWithNullArgument(nameof(input));
        }

        if (input.UpdateRequest.SelectAllState != MyPupilsFormSelectionModeRequestDto.SelectAll)
        {
            return HandlerResultValueTaskFactory.Skipped();
        }

        input.State.SelectAll();

        return HandlerResultValueTaskFactory.SuccessfulResult();
    }
}
