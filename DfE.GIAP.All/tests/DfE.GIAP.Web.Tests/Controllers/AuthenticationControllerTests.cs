using System.Security.Claims;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Config;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers;

[Trait("Category", "Authentication Controller Unit Tests")]

public sealed class AuthenticationControllerTests
{
    private readonly Mock<IOptions<DsiOptions>> _mockAzureAppSettings = new();

    private AuthenticationController GetAuthenticationController()
    {
        _mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new DsiOptions() { RedirectUrlAfterSignout = "http://redirectToSomewhere.com" });
        return new AuthenticationController(_mockAzureAppSettings.Object);
    }

    [Fact]
    public void AuthenticationController_LoginDSI_SetDefaultRedirectURL_If_Not_Authenticated()
    {
        // Arrange
        string? redirectUrl = null;
        string expectedURL = "https://giapBaseDomain/";
        ControllerContext context = new() { HttpContext = new DefaultHttpContext() };
        AuthenticationController controller = GetAuthenticationController();

        Mock<IUrlHelper> mockUrlHelper = new();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(expectedURL);

        controller.Url = mockUrlHelper.Object;
        controller.ControllerContext = context;

        // Act
        ChallengeResult? result = controller.LoginDsi(redirectUrl) as ChallengeResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedURL, result.Properties!.RedirectUri);
    }

    [Fact]
    public void AuthenticationController_LoginDSI_SetRedirectURL_If_Not_Authenticated()
    {
        // Arrange
        ControllerContext context = new() { HttpContext = new DefaultHttpContext() };
        AuthenticationController controller = GetAuthenticationController();
        controller.ControllerContext = context;
        string redirectUrl = "http://redirectToSomewhere.com";

        // Act
        ChallengeResult? result = controller.LoginDsi(redirectUrl) as ChallengeResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(redirectUrl, result.Properties!.RedirectUri);
    }

    [Fact]
    public void AuthenticationController_LoginDSI_Redirect_If_Authenticated()
    {
        // Arrange
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new() { HttpContext = new DefaultHttpContext() { User = user, Session = new SessionFake() } };
        AuthenticationController controller = GetAuthenticationController();
        controller.ControllerContext = context;
        string redirectUrl = "http://redirectToSomewhere.com";

        // Act
        IActionResult result = controller.LoginDsi(redirectUrl);

        // Assert
        RedirectResult redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.NotNull(redirectResult);
        Assert.Equal(redirectUrl, redirectResult.Url);
    }

    [Fact]
    public async Task AuthenticationController_SignoutDSI_And_Redirect()
    {
        // Arrange
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetUserClaimsPrincipal();

        Mock<IAuthenticationService> authServiceMock = new();
        authServiceMock
            .Setup(_ => _.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.FromResult((object?)null));

        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock
            .Setup(_ => _.GetService(typeof(IAuthenticationService)))
            .Returns(authServiceMock.Object);

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext()
            {
                User = user,
                Session = new SessionFake(),
                RequestServices = serviceProviderMock.Object
            }
        };
        AuthenticationController controller = GetAuthenticationController();
        controller.ControllerContext = context;
        string dsiRedirectUrlAfterSignout = "http://redirectToSomewhere.com";

        // Act
        IActionResult result = await controller.SignoutDsi();

        // Assert
        RedirectResult redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.NotNull(redirectResult);
        Assert.Equal(dsiRedirectUrlAfterSignout, redirectResult.Url);
    }
}
