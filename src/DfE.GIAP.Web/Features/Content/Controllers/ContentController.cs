using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Content.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.Content.Controllers;

[Authorize(Policy = Policy.RequiresManageContentAccess)]
public class ContentController : Controller
{
    public IActionResult Index()
    {
        ContentIndexViewModel model = new()
        {
            Options = new List<ContentOptionViewModel>
            {
                new() { Value = ContentManagementOption.ManageNews, DisplayName = "Manage News" },
            }
        };

        return View(model);
    }


    [HttpPost]
    public IActionResult Index(ContentIndexViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Options = new List<ContentOptionViewModel>
            {
                new() { Value = ContentManagementOption.ManageNews, DisplayName = "Manage News" },
            };
            return View(model);
        }

        return model.SelectedOption switch
        {
            ContentManagementOption.ManageNews => RedirectToAction("ManageNewsArticles", "ManageNewsArticles"),
            _ => RedirectToAction("Index")
        };
    }
}
