using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presentation.PupilDtoPresentationHandlers.Options;

public sealed class PupilsPresentationOptionsProvider : IPupilsPresentationOptionsProvider
{
    private readonly ISessionProvider _sessionProvider;
    public PupilsPresentationOptionsProvider(ISessionProvider sessionProvider)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;
    }

    public PupilsPresentationOptions GetOptions()
    {
        PupilsPresentationOptions options =
            _sessionProvider.ContainsSessionKey(nameof(PupilsPresentationOptions)) ?
                _sessionProvider.GetSessionValueOrDefault<PupilsPresentationOptions>(nameof(PupilsPresentationOptions)) :
                    new(
                        Page: 1,
                        SortBy: string.Empty,
                        SortDirection: SortDirection.Descending);

        return options;
    }

    public void SetOptions(PupilsPresentationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _sessionProvider.SetSessionValue(nameof(PupilsPresentationOptions), options);
    }
}
