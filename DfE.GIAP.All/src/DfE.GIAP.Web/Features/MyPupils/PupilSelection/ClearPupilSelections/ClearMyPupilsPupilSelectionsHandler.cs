using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Options;
using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Abstraction.Command;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.ClearPupilSelections;

internal sealed class ClearMyPupilsPupilSelectionsHandler : IClearMyPupilsPupilSelectionsHandler
{
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _sessionCommandHandler;
    private readonly SessionCacheKey _sessionCacheKey;

    public ClearMyPupilsPupilSelectionsHandler(
        ISessionCommandHandler<MyPupilsPupilSelectionState> sessionCommandHandler,
        IOptions<MyPupilSelectionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(sessionCommandHandler);
        _sessionCommandHandler = sessionCommandHandler;

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);

        _sessionCacheKey = new(options.Value.SelectionsSessionKey);
    }

    public void Handle()
    {
        _sessionCommandHandler.StoreInSession(
            sessionCacheKey: _sessionCacheKey,
            value: MyPupilsPupilSelectionState.CreateDefault());
    }
}
