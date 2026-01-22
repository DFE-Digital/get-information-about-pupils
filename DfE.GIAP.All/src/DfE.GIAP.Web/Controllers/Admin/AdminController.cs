using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers.Admin;

[Authorize(Policy = Policy.RequiresAdminApproverAccess)]
public class AdminController : Controller
{
    private const string AdminIndexViewPath = "../Admin/Index";
    public IActionResult Index()
    {
        return View(AdminIndexViewPath, GetAdminViewModel());
    }

    [HttpPost]
    [Route(Routes.Admin.AdminOptions)]
    public IActionResult AdminOptions(AdminViewModel model)
    {
        if (string.IsNullOrEmpty(model.SelectedAdminOption))
        {
            ModelState.AddModelError("NoAdminSelection", Messages.Common.Errors.NoAdminSelection);
            return View(AdminIndexViewPath, GetAdminViewModel());
        }

        if(model.SelectedAdminOption == "ManageNewsArticles")
        {
            return RedirectToAction("ManageNewsArticles", "ManageNewsArticles");
        }

        return View(AdminIndexViewPath, GetAdminViewModel());
    }

    // TODO: Check how this model is being used in the views, do we really need this?
    private AdminViewModel GetAdminViewModel()
    {
        return new AdminViewModel
        {
            IsAdmin = User.IsAdmin(),
        };
    }
}
