using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Controllers.MyPupilList.PresentationState.Provider;

public sealed class MyPupilsPresentationStateProvider : IMyPupilsPresentationStateProvider
{
    private readonly ISessionProvider _sessionProvider;

    private static string SessionKey => nameof(MyPupilsPresentationState);
    public MyPupilsPresentationStateProvider(ISessionProvider sessionProvider)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;
    }

    public MyPupilsPresentationState Get()
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

    public void Set(MyPupilsPresentationState options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _sessionProvider.SetSessionValue(SessionKey, options);
    }
}
