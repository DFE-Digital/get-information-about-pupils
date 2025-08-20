using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;

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
                    PupilsPresentationOptions.Default;

        return options;
    }

    public void SetOptions(PupilsPresentationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _sessionProvider.SetSessionValue(nameof(PupilsPresentationOptions), options);
    }
}
