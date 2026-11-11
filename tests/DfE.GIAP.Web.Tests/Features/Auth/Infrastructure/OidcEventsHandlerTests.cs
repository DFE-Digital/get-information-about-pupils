using System.Security.Claims;
using DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;
using DfE.GIAP.Web.Features.Auth.Infrastructure;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Auth.Infrastructure;

public class OidcEventsHandlerTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WithNullHandlers()
    {
        IOptions<DsiOptions> options = Options.Create(new DsiOptions { SessionTimeoutMinutes = 30 });

        Assert.Throws<ArgumentNullException>(() =>
            new OidcEventsHandler(null!, options));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WithNullOptionsValue()
    {
        IReadOnlyList<IPostTokenValidatedHandler> handlers = new List<IPostTokenValidatedHandler>();
        IOptions<DsiOptions> options = Options.Create<DsiOptions>(null!);

        Assert.Throws<ArgumentNullException>(() =>
            new OidcEventsHandler(handlers, options));
    }

    [Fact]
    public async Task OnTokenValidated_CallsHandlers_And_UpdatesProperties()
    {
        // Arrange
        Mock<IPostTokenValidatedHandler> mockHandler = new Mock<IPostTokenValidatedHandler>();
        List<IPostTokenValidatedHandler> handlers = new List<IPostTokenValidatedHandler> { mockHandler.Object };
        IOptions<DsiOptions> options = Options.Create(new DsiOptions { SessionTimeoutMinutes = 45 });

        OidcEventsHandler sut = new OidcEventsHandler(handlers, options);

        ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "user-123") }));
        TokenValidatedContext context = TokenValidatedContextFactory.Create(principal);

        // Act
        await sut.OnTokenValidated(context);

        // Assert
        Assert.NotNull(context.Properties);
        Assert.True(context.Properties.IsPersistent);
        Assert.NotNull(context.Properties.ExpiresUtc);
        mockHandler.Verify(h => h.HandleAsync(It.Is<TokenAuthorisationContext>(c => c.Principal == principal)), Times.Once);
    }
}
