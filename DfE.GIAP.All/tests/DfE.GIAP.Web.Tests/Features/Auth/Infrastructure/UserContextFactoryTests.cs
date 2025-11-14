using System.Security.Claims;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Features.Auth.Infrastructure;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Auth.Infrastructure;

public class UserContextFactoryTests
{
    [Fact]
    public void FromPrincipal_WithAllRoles_ShouldSetAllFlagsTrue()
    {
        // Arrange
        List<Claim> claims = new()
        {
            new Claim(AuthClaimTypes.UserId, "user-123"),
            new Claim(ClaimTypes.Role, AuthRoles.Admin),
            new Claim(ClaimTypes.Role, AuthRoles.Approver),
            new Claim(ClaimTypes.Role, AuthRoles.User)
        };
        ClaimsIdentity identity = new(claims);
        ClaimsPrincipal principal = new(identity);

        // Act
        AuthenticatedUser result = UserContextFactory.FromPrincipal(principal);

        // Assert
        Assert.Equal("user-123", result.UserId);
        Assert.True(result.IsAdmin);
        Assert.True(result.IsApprover);
        Assert.True(result.IsUser);
    }

    [Fact]
    public void FromPrincipal_WithOnlyAdminRole_ShouldSetOnlyAdminTrue()
    {
        // Arrange
        List<Claim> claims = new()
        {
            new Claim(AuthClaimTypes.UserId, "admin-user"),
            new Claim(ClaimTypes.Role, AuthRoles.Admin)
        };
        ClaimsIdentity identity = new(claims);
        ClaimsPrincipal principal = new(identity);

        // Act
        AuthenticatedUser result = UserContextFactory.FromPrincipal(principal);

        // Assert
        Assert.Equal("admin-user", result.UserId);
        Assert.True(result.IsAdmin);
        Assert.False(result.IsApprover);
        Assert.False(result.IsUser);
    }

    [Fact]
    public void FromPrincipal_WithNoRoles_ShouldSetAllFlagsFalse()
    {
        // Arrange
        List<Claim> claims = new()
        {
            new Claim(AuthClaimTypes.UserId, "no-role-user")
        };
        ClaimsIdentity identity = new(claims);
        ClaimsPrincipal principal = new(identity);

        // Act
        AuthenticatedUser result = UserContextFactory.FromPrincipal(principal);

        // Assert
        Assert.Equal("no-role-user", result.UserId);
        Assert.False(result.IsAdmin);
        Assert.False(result.IsApprover);
        Assert.False(result.IsUser);
    }

    [Fact]
    public void FromPrincipal_WithMissingUserIdClaim_ShouldSetUserIdToEmpty()
    {
        // Arrange
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Role, AuthRoles.User)
        };
        ClaimsIdentity identity = new(claims);
        ClaimsPrincipal principal = new(identity);

        // Act
        AuthenticatedUser result = UserContextFactory.FromPrincipal(principal);

        // Assert
        Assert.Equal(string.Empty, result.UserId);
        Assert.False(result.IsAdmin);
        Assert.False(result.IsApprover);
        Assert.True(result.IsUser);
    }
}
