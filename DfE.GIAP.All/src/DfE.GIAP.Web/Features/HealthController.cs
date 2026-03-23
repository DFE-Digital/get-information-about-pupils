using DfE.GIAP.Web.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features;

[AllowAnonymous]
[AllowWithoutConsent]
public class HealthController : Controller
{
    public IActionResult Index() => Ok("Ok");
}
