using System.Security.Claims;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Home;

public class HomeControllerTests : IClassFixture<UserClaimsPrincipalFake>
{
    private readonly UserClaimsPrincipalFake _userClaimsPrincipalFake;
    private readonly ILatestNewsBanner _mockNewsBanner = new Mock<ILatestNewsBanner>().Object;
    private readonly Mock<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>> _mockGetContentByPageKeyUseCase = new();
    private readonly IExceptionHandlerPathFeature _exceptionPathFeature = Substitute.For<IExceptionHandlerPathFeature>();

    public HomeControllerTests(UserClaimsPrincipalFake userClaimsPrincipalFake)
    {
        _userClaimsPrincipalFake = userClaimsPrincipalFake;
    }


    [Fact]
    public void Error404_returns_view()
    {
        // Arrange
        var controller = GetHomeController();

        // Act
        var result = controller.Error404();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_returns_view()
    {
        // Arrange
        var controller = GetHomeController();

        // Act
        var result = controller.Error(500);

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void UserWithNoRole_returns_view()
    {
        // Arrange
        var controller = GetHomeController();

        // Act
        var result = controller.UserWithNoRole();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Exception_page_doesnt_show_error_in_production()
    {
        // Arrange
        var controller = GetHomeController();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

        // Act
        var result = controller.Exception();

        // Assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;
        Assert.IsType<ErrorModel>(viewResult.Model);
        var viewModel = viewResult.Model as ErrorModel;
        Assert.False(viewModel.ShowError);
    }

    [Fact]
    public async Task HomeController_Index_Should_Return_HomePageData()
    {
        // Arrange
        Content stubContent = new()
        {
            Title = "Test title",
            Body = "Test body",
        };

        HomeViewModel model = new()
        {
            LandingResponse = stubContent,
        };


        GetContentByPageKeyUseCaseResponse response = new(stubContent);

        _mockGetContentByPageKeyUseCase.Setup(
            (useCase) => useCase.HandleRequestAsync(
                It.IsAny<GetContentByPageKeyUseCaseRequest>())).ReturnsAsync(response).Verifiable();

        HomeController controller = GetHomeController();

        // Act
        IActionResult result = await controller.Index();

        // Assert
        ViewResult viewResult = Assert.IsAssignableFrom<ViewResult>(result);
        HomeViewModel viewModel = viewResult.Model as HomeViewModel;
        Assert.Equal(model.LandingResponse.Title, viewModel.LandingResponse.Title);
        Assert.Equal(model.LandingResponse.Body, viewModel.LandingResponse.Body);
    }

    [Fact]
    public void HomeController_IndexPost_Should_Redirect_To_NPD_Search_If_User_Is_Not_An_FE_User()
    {
        // Arrange
        var controller = GetHomeController();

        // Act
        var result = controller.IndexPost();

        // Assert
        var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
        Assert.Equal(Global.NPDAction, viewResult.ActionName);
        Assert.Equal(Global.NPDLearnerNumberSearchController, viewResult.ControllerName);
    }

    [Fact]
    public void HomeController_IndexPost_Should_Redirect_To_FE_Search_If_User_Is_An_FE_User()
    {
        // Arrange
        var controller = GetHomeController(true);

        // Act
        var result = controller.IndexPost();

        // Assert
        var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
        Assert.Equal(Global.FELearnerNumberSearchAction, viewResult.ActionName);
        Assert.Equal(Global.FELearnerNumberSearchController, viewResult.ControllerName);
    }

    private HomeController GetHomeController(bool feUser = false)
    {
        ClaimsPrincipal user;

        if (feUser)
        {
            user = _userClaimsPrincipalFake.GetSpecificUserClaimsPrincipal(
                DsiKeys.OrganisationCategory.Establishment,
                DsiKeys.EstablishmentType.FurtherEducation,
                Roles.Approver,
                18,
                25);
        }
        else
        {
            user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        }

        _exceptionPathFeature.Error.Returns(new Exception("test"));
        _exceptionPathFeature.Path.Returns("/");

        var controllerContext = new ControllerContext();
        controllerContext.HttpContext = new DefaultHttpContext();

        controllerContext.HttpContext.Features.Set(_exceptionPathFeature);

        return new HomeController(_mockNewsBanner, _mockGetContentByPageKeyUseCase.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            }
        };
    }
}
