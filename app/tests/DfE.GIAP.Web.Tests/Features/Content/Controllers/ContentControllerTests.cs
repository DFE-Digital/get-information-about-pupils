using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Content;
using DfE.GIAP.Web.Features.Content.Controllers;
using DfE.GIAP.Web.Features.Content.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Content.Controllers;

public class ContentControllerTests
{
    [Fact]
    public void Index_Get_Returns_ViewWithExpectedModel()
    {
        ContentController controller = new();

        IActionResult result = controller.Index();

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        ContentIndexViewModel model = Assert.IsType<ContentIndexViewModel>(viewResult.Model);

        Assert.NotNull(model.Options);
        Assert.Single(model.Options);

        ContentOptionViewModel option = model.Options[0];
        Assert.Equal(ContentManagementOption.ManageNews, option.Value);
        Assert.Equal("Manage News", option.DisplayName);
    }


    [Fact]
    public void Index_Post_InvalidModelState_Returns_ViewWithOptionsReset()
    {
        ContentController controller = new ContentController();
        controller.ModelState.AddModelError("Test", "Error");

        ContentIndexViewModel inputModel = new ContentIndexViewModel();

        IActionResult result = controller.Index(inputModel);

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        ContentIndexViewModel model = Assert.IsType<ContentIndexViewModel>(viewResult.Model);

        Assert.NotNull(model.Options);
        Assert.Single(model.Options);

        ContentOptionViewModel option = model.Options[0];
        Assert.Equal(ContentManagementOption.ManageNews, option.Value);
        Assert.Equal("Manage News", option.DisplayName);
    }

    [Fact]
    public void Index_Post_ManageNewsSelected_RedirectsToManageNewsArticles()
    {
        ContentController controller = new();

        ContentIndexViewModel model = new()
        {
            SelectedOption = ContentManagementOption.ManageNews
        };

        IActionResult result = controller.Index(model);

        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ManageNewsArticles", redirect.ActionName);
        Assert.Equal("ManageNewsArticles", redirect.ControllerName);
    }

    [Fact]
    public void Index_Post_UnknownOption_RedirectsToIndex()
    {
        ContentController controller = new();

        ContentIndexViewModel model = new()
        {
            SelectedOption = (ContentManagementOption)999 // unknown
        };

        IActionResult result = controller.Index(model);

        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void Controller_HasAuthorizeAttribute_WithCorrectPolicy()
    {
        Type controllerType = typeof(ContentController);

        AuthorizeAttribute? attribute = controllerType
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal(Policy.RequiresManageContentAccess, attribute.Policy);
    }
}
