using DfE.GIAP.Web.Features.MyPupils.PresentationState;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationState.Provider;

public sealed class MyPupilsPresentationStateProvider : IMyPupilsPresentationStateProvider
{
    private readonly ISessionProvider _sessionProvider;

    private static string SessionKey => nameof(MyPupilsPresentationState);
    public MyPupilsPresentationStateProvider(ISessionProvider sessionProvider)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;
    }

    public MyPupilsPresentationState GetState()
    {
        MyPupilsPresentationState options =
            _sessionProvider.ContainsSessionKey(SessionKey) ?
                _sessionProvider.GetSessionValueOrDefault<MyPupilsPresentationState>(SessionKey) :
                    new(
                        Page: 1,
                        SortBy: string.Empty,
                        SortDirection: SortDirection.Descending);

        return options;
    }

    public void SetState(MyPupilsPresentationState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _sessionProvider.SetSessionValue(SessionKey, state);
    }
}
