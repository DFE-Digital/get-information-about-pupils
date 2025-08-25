using System.Security.Claims;
using System.Text.Encodings.Web;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Web.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.IntegrationTests.WebApplicationFactory;
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string TestScheme = "TestScheme";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                           ILoggerFactory logger,
                           UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Claim[] claims =
        [
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, Roles.Admin),
            new Claim(CustomClaimTypes.UserId, "MY_DSI_ID"),
            // Add more claims as needed
        ];

        ClaimsIdentity identity = new(claims, "DfE-SignIn");
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, "DfE-SignIn");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
