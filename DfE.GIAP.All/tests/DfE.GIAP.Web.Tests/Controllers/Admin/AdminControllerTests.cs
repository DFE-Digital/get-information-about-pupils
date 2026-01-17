using System.Security.Claims;
using DfE.GIAP.Web.Controllers.Admin;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Admin;

[Trait("Category", "Admin Controller Unit Tests")]
public sealed class AdminControllerTests
{
    [Fact]
    public void AdminController_AdminViewLoadsSuccessfully()
    {
        // Arrange
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        AdminController controller = new()
        {
            ControllerContext = context
        };

        // Act
        IActionResult result = controller.Index();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/Index", viewResult.ViewName);
    }

    [Fact]
    public void AdminController_DashboardOptions_Returns_ManageDocuments_Redirect_To_Action()
    {
        // Arrange
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = new()
        {
            ControllerContext = context
        };

        AdminViewModel model = new()
        {
            SelectedAdminOption = "ManageNewsArticles"
        };

        // Act
        IActionResult result = controller.AdminOptions(model);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal("ManageNewsArticles", viewResult.ControllerName);
        Assert.Equal("ManageNewsArticles", viewResult.ActionName);
    }

    [Fact]
    public void AdminController_DashboardOptions_Returns_ValidationMessage_If_No_Selection_Made()
    {
        // Arrange
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = new()
        {
            ControllerContext = context
        };

        AdminViewModel model = new()
        {
            SelectedAdminOption = null
        };

        // Act
        IActionResult result = controller.AdminOptions(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        AdminViewModel? viewModel = viewResult.Model as AdminViewModel;
        Assert.NotNull(viewModel);
        Assert.Equal("../Admin/Index", viewResult.ViewName);
        Assert.Single(controller.ViewData.ModelState["NoAdminSelection"]!.Errors);
    }
}
