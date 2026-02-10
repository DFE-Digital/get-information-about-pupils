using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Options;
using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Abstraction.Command;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;

internal sealed class UpdateMyPupilsPupilSelectionsCommandHandler : IUpdateMyPupilsPupilSelectionsCommandHandler
{
    private readonly IGetMyPupilsPupilSelectionProvider _getPupilSelectionsProvider;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _pupilSelectionStateCommandHandler;
    private readonly IEvaluator<UpdateMyPupilsSelectionStateRequest> _evaluator;
    private readonly SessionCacheKey _selectionsSessionCacheKey;

    public UpdateMyPupilsPupilSelectionsCommandHandler(
        IGetMyPupilsPupilSelectionProvider getPupilSelectionsProvider,
        ISessionCommandHandler<MyPupilsPupilSelectionState> pupilSelectionStateCommandHandler,
        IEvaluator<UpdateMyPupilsSelectionStateRequest> evaluator,
        IOptions<MyPupilSelectionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(getPupilSelectionsProvider);
        _getPupilSelectionsProvider = getPupilSelectionsProvider;

        ArgumentNullException.ThrowIfNull(pupilSelectionStateCommandHandler);
        _pupilSelectionStateCommandHandler = pupilSelectionStateCommandHandler;

        ArgumentNullException.ThrowIfNull(evaluator);
        _evaluator = evaluator;

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);
        _selectionsSessionCacheKey = new(options.Value.SelectionsSessionKey);
    }

    public async Task Handle(MyPupilsFormStateRequestDto formDto)
    {
        UpdateMyPupilsSelectionStateRequest updateRequest = new(formDto, _getPupilSelectionsProvider.GetPupilSelections());

        await _evaluator.EvaluateAsync(updateRequest);

        _pupilSelectionStateCommandHandler.StoreInSession(
            _selectionsSessionCacheKey,
            updateRequest.State);
    }
}

