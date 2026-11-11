using System.Security.Claims;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Config;
using DfE.GIAP.Web.Features.Auth.Presentation.Controllers;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Tests.Features.Auth;

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
