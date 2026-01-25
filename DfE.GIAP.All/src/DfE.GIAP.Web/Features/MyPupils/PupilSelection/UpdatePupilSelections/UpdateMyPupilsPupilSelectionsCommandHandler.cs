using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;
using DfE.GIAP.Web.Shared.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;

public class UpdateMyPupilsPupilSelectionsCommandHandler : IUpdateMyPupilsPupilSelectionsCommandHandler
{
    private readonly IGetMyPupilsPupilSelectionProvider _getPupilSelectionsProvider;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _pupilSelectionStateCommandHandler;
    private readonly IEvaluator<UpdateMyPupilsSelectionStateRequest> _evaluator;
    public UpdateMyPupilsPupilSelectionsCommandHandler(
        IGetMyPupilsPupilSelectionProvider getPupilSelectionsProvider,
        ISessionCommandHandler<MyPupilsPupilSelectionState> pupilSelectionStateCommandHandler,
        IEvaluator<UpdateMyPupilsSelectionStateRequest> evaluator)
    {
        ArgumentNullException.ThrowIfNull(getPupilSelectionsProvider);
        _getPupilSelectionsProvider = getPupilSelectionsProvider;

        ArgumentNullException.ThrowIfNull(pupilSelectionStateCommandHandler);
        _pupilSelectionStateCommandHandler = pupilSelectionStateCommandHandler;

        ArgumentNullException.ThrowIfNull(evaluator);
        _evaluator = evaluator;
    }

    public async Task Handle(MyPupilsFormStateRequestDto formDto)
    {
        UpdateMyPupilsSelectionStateRequest updateRequest = new(formDto, _getPupilSelectionsProvider.GetPupilSelections());

        await _evaluator.EvaluateAsync(updateRequest);

        _pupilSelectionStateCommandHandler.StoreInSession(updateRequest.State);
    }
}

