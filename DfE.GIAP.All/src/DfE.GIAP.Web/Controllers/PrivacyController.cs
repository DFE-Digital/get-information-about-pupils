using DfE.GIAP.Web.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class PrivacyController : Controller
{
    public PrivacyController()
    {
    }

    [AllowWithoutConsent]
    public IActionResult Index()
    {
        return View();
    }
}
