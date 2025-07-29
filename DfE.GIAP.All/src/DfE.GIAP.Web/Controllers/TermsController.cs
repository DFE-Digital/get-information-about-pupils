using DfE.GIAP.Web.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class TermsController : Controller
{
    public TermsController()
    {
    }

    [AllowWithoutConsent]
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
