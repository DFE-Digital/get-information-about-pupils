using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandler;
using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;
using DfE.GIAP.Web.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;

public class UpdateMyPupilsPupilSelectionsCommandHandler : IUpdateMyPupilsPupilSelectionsCommandHandler
{
    private readonly IGetMyPupilsPupilSelectionProvider _getPupilSelectionsProvider;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _pupilSelectionStateCommandHandler;
    private readonly IEvaluationHandler<UpdateMyPupilsSelectionStateRequest> _evaluator;
    public UpdateMyPupilsPupilSelectionsCommandHandler(
        IGetMyPupilsPupilSelectionProvider getPupilSelectionsProvider,
        ISessionCommandHandler<MyPupilsPupilSelectionState> pupilSelectionStateCommandHandler,
        IEvaluationHandler<UpdateMyPupilsSelectionStateRequest> evaluator)
    {
        ArgumentNullException.ThrowIfNull(getPupilSelectionsProvider);
        _getPupilSelectionsProvider = getPupilSelectionsProvider;

        ArgumentNullException.ThrowIfNull(pupilSelectionStateCommandHandler);
        _pupilSelectionStateCommandHandler = pupilSelectionStateCommandHandler;

        ArgumentNullException.ThrowIfNull(evaluator);
        _evaluator = evaluator;
    }

    public void Handle(MyPupilsFormStateRequestDto formDto)
    {
        UpdateMyPupilsSelectionStateRequest updateRequest = new(formDto, _getPupilSelectionsProvider.GetPupilSelections());

        _evaluator.Evaluate(updateRequest);

        _pupilSelectionStateCommandHandler.StoreInSession(updateRequest.State);
    }
}