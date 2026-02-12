using System.Security.Claims;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features;

public sealed class HomeControllerTests
{
    private readonly IExceptionHandlerPathFeature _exceptionPathFeature = Substitute.For<IExceptionHandlerPathFeature>();


    [Fact]
    public void Error404_returns_view()
    {
        // Arrange
        HomeController controller = GetHomeController();

        // Act
        IActionResult result = controller.Error404();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_returns_view()
    {
        // Arrange
        HomeController controller = GetHomeController();

        // Act
        IActionResult result = controller.Error(500);

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void UserWithNoRole_returns_view()
    {
        // Arrange
        HomeController controller = GetHomeController();

        // Act
        IActionResult result = controller.UserWithNoRole();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Exception_page_doesnt_show_error_in_production()
    {
        // Arrange
        HomeController controller = GetHomeController();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

        // Act
        IActionResult result = controller.Exception();

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;
        Assert.NotNull(viewResult);
        Assert.IsType<ErrorModel>(viewResult.Model);
        ErrorModel? viewModel = viewResult.Model as ErrorModel;
        Assert.NotNull(viewModel);
        Assert.False(viewModel.ShowError);
    }

    [Fact]
    public void HomeController_IndexPost_Should_Redirect_To_NPD_Search_If_User_Is_Not_An_FE_User()
    {
        // Arrange
        HomeController controller = GetHomeController();

        // Act
        IActionResult result = controller.IndexPost();

        // Assert
        RedirectToActionResult viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
        Assert.Equal(Global.NPDAction, viewResult.ActionName);
        Assert.Equal(Global.NPDLearnerNumberSearchController, viewResult.ControllerName);
    }

    [Fact]
    public void HomeController_IndexPost_Should_Redirect_To_FE_Search_If_User_Is_An_FE_User()
    {
        // Arrange
        HomeController controller = GetHomeController(true);

        // Act
        IActionResult result = controller.IndexPost();

        // Assert
        RedirectToActionResult viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
        Assert.Equal(Global.FELearnerNumberSearchAction, viewResult.ActionName);
        Assert.Equal(Global.FELearnerNumberSearchController, viewResult.ControllerName);
    }

    private HomeController GetHomeController(bool feUser = false)
    {
        ClaimsPrincipal user;

        if (feUser)
        {
            user = UserClaimsPrincipalFake.GetSpecificUserClaimsPrincipal(
                DsiKeys.OrganisationCategory.Establishment,
                DsiKeys.EstablishmentType.FurtherEducation,
                AuthRoles.Approver,
                18,
                25);
        }
        else
        {
            user = UserClaimsPrincipalFake.GetUserClaimsPrincipal();
        }

        _exceptionPathFeature.Error.Returns(new Exception("test"));
        _exceptionPathFeature.Path.Returns("/");

        ControllerContext controllerContext = new ControllerContext();
        controllerContext.HttpContext = new DefaultHttpContext();

        controllerContext.HttpContext.Features.Set(_exceptionPathFeature);

        return new HomeController()
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            }
        };
    }
}
