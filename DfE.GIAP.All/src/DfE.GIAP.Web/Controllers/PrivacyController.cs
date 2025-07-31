using DfE.GIAP.Web.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class PrivacyController : Controller
{
    [AllowWithoutConsent]
    public IActionResult Index()
    {
        return View();
    }
}
