using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v1;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;
using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections.Handlers;

internal sealed class SelectAllPupilsCommandHandler : IEvaluationHandlerV2<UpdateMyPupilsSelectionStateRequest>
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
