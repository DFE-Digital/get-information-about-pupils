using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Providers.NewsBanner;

public class NewsBannerProvider : INewsBannerProvider
{
    private readonly ISessionProvider _sessionProvider;

    public NewsBannerProvider(ISessionProvider sessionProvider)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;
    }

    public bool ShouldShowNewsBanner()
    {
        string ShowNewsBannerValue = _sessionProvider.GetSessionValue(SessionKeys.ShowNewsBanner);
        bool showNewsBanner = ShowNewsBannerValue == SessionKeys.ShowNewsBanner;

        string consentValue = _sessionProvider.GetSessionValue(SessionKeys.ConsentGiven);
        bool hasGivenConsent = consentValue == SessionKeys.ConsentGiven;

        return showNewsBanner && hasGivenConsent;
    }
}
