using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;

public sealed class PresentPupilOptionsProvider : IPresentPupilOptionsProvider
{
    private readonly ISessionProvider _sessionProvider;
    public PresentPupilOptionsProvider(ISessionProvider sessionProvider)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;
    }

    public PresentPupilsOptions GetOptions()
    {
        PresentPupilsOptions options =
            _sessionProvider.ContainsSessionKey(nameof(PresentPupilsOptions)) ?
                _sessionProvider.GetSessionValueOrDefault<PresentPupilsOptions>(nameof(PresentPupilsOptions)) :
                    PresentPupilsOptions.Default;

        return options;
    }

    public void SetOptions(PresentPupilsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _sessionProvider.SetSessionValue(nameof(PresentPupilsOptions), options);
    }
}
