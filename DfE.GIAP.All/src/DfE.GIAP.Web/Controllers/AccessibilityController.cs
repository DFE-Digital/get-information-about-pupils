using DfE.GIAP.Web.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class AccessibilityController : Controller
{
    [AllowWithoutConsent]
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Report()
    {
        return View();
    }
}
