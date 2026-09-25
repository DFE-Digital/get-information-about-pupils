using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Config;
using DfE.GIAP.Web.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.Auth.Presentation.Controllers;

[Route(Routes.Authentication.AuthenticationController)]
public class AuthenticationController : Controller
{
    private readonly DsiOptions _dsiOptions;

    public AuthenticationController(IOptions<DsiOptions> dsiOptions)
    {
        ArgumentNullException.ThrowIfNull(dsiOptions.Value);
        _dsiOptions = dsiOptions.Value;
    }

    [AllowWithoutConsent]
    [AllowAnonymous]
    [HttpGet(Routes.Authentication.SignoutAction)]
    public async Task<IActionResult> SignoutDsi()
    {
        HttpContext.Session.Clear();

        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Redirect(_dsiOptions.RedirectUrlAfterSignout);
    }
}
