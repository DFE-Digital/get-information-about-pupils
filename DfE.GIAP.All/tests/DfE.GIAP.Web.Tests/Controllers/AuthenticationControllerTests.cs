using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Config;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers;

[Trait("Category", "Authentication Controller Unit Tests")]

public class AuthenticationControllerTests : IClassFixture<UserClaimsPrincipalFake>
{
    private readonly Mock<IOptions<DsiOptions>> _mockAzureAppSettings = new Mock<IOptions<DsiOptions>>();
    private ISession _mockSession = Substitute.For<ISession>();

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
        string redirectUrl = null;
        string expectedURL = "https://giapBaseDomain/";
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() };
        var controller = GetAuthenticationController();

        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(expectedURL);

        controller.Url = mockUrlHelper.Object;
        controller.ControllerContext = context;

        // Act
        var result = controller.LoginDsi(redirectUrl) as ChallengeResult;

        // Assert
        Assert.IsType<ChallengeResult>(result);
        Assert.Equal(expectedURL, result.Properties.RedirectUri);
    }

    [Fact]
    public void AuthenticationController_LoginDSI_SetRedirectURL_If_Not_Authenticated()
    {
        // Arrange
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() };
        var controller = GetAuthenticationController();
        controller.ControllerContext = context;
        string redirectUrl = "http://redirectToSomewhere.com";

        // Act
        var result = controller.LoginDsi(redirectUrl) as ChallengeResult;

        // Assert
        Assert.IsType<ChallengeResult>(result);
        Assert.Equal(redirectUrl, result.Properties.RedirectUri);
    }

    [Fact]
    public void AuthenticationController_LoginDSI_Redirect_If_Authenticated()
    {
        // Arrange
        var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession } };
        var controller = GetAuthenticationController();
        controller.ControllerContext = context;
        string redirectUrl = "http://redirectToSomewhere.com";

        // Act
        var result = controller.LoginDsi(redirectUrl);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.NotNull(redirectResult);
        Assert.Equal(redirectUrl, redirectResult.Url);
    }

    [Fact]
    public async Task AuthenticationController_SignoutDSI_And_Redirect()
    {
        // Arrange
        var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();

        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(_ => _.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.FromResult((object)null));

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(_ => _.GetService(typeof(IAuthenticationService)))
            .Returns(authServiceMock.Object);

        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession, RequestServices = serviceProviderMock.Object } };
        var controller = GetAuthenticationController();
        controller.ControllerContext = context;
        string dsiRedirectUrlAfterSignout = "http://redirectToSomewhere.com";

        // Act
        var result = await controller.SignoutDsi().ConfigureAwait(false);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.NotNull(redirectResult);
        Assert.Equal(dsiRedirectUrlAfterSignout, redirectResult.Url);
    }
}
