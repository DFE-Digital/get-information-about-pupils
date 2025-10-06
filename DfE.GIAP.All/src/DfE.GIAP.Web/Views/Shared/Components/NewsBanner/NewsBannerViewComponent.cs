using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Providers.Session;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Views.Shared.Components.NewsBanner;

public class NewsBannerViewModel
{
    public bool ConsentGiven { get; set; }
    public bool ShowBanner { get; set; }
}

public class NewsBannerViewComponent : ViewComponent
{
    public const string Name = "NewsBanner";
    private readonly ISessionProvider _sessionProvider;

    public NewsBannerViewComponent(ISessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public IViewComponentResult Invoke()
    {
        bool consentGiven = _sessionProvider.GetSessionValueOrDefault<bool>(SessionKeys.ConsentGivenKey);
        bool showBanner = _sessionProvider.GetSessionValueOrDefault<bool>(SessionKeys.ShowNewsBannerKey);

        NewsBannerViewModel model = new()
        {
            ConsentGiven = consentGiven,
            ShowBanner = showBanner
        };

        return View(model);
    }
}
