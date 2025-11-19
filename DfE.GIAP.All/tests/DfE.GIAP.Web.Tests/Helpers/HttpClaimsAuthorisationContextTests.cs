using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Helpers;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers;

public sealed class HttpClaimsAuthorisationContextTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenUserIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HttpClaimsAuthorisationContext(null!));
    }

    [Theory]
    [InlineData(AuthRoles.Admin)]
    [InlineData(AuthRoles.Approver)]
    [InlineData(AuthRoles.User)]
    public void Role_ReturnsExpectedRole_WhenUserIsInRole(string expectedRole)
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, expectedRole)
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.Equal(expectedRole, context.Role);
    }

    [Fact]
    public void Role_ReturnsEmpty_WhenUserHasNoRecognisedRole()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Guest")
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.Equal(string.Empty, context.Role);
    }

    [Fact]
    public void IsDfeUser_ReturnsTrue_WhenOrganisationNameMatches()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationName, DsiKeys.Common.DepartmentForEducation)
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.True(context.IsDfeUser);
    }

    [Fact]
    public void IsDfeUser_ReturnsFalse_WhenOrganisationNameDoesNotMatch()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationName, "Some Other Org")
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.False(context.IsDfeUser);
    }

    [Fact]
    public void StatutoryAgeLow_ReturnsParsedValue_WhenValid()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationLowAge, "5")
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.Equal(5, context.StatutoryAgeLow);
    }

    [Fact]
    public void StatutoryAgeLow_ReturnsZero_WhenInvalid()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationLowAge, "invalid")
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.Equal(0, context.StatutoryAgeLow);
    }

    [Fact]
    public void StatutoryAgeHigh_ReturnsParsedValue_WhenValid()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationHighAge, "18")
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.Equal(18, context.StatutoryAgeHigh);
    }

    [Fact]
    public void StatutoryAgeHigh_ReturnsZero_WhenInvalid()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationHighAge, "invalid")
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.Equal(0, context.StatutoryAgeHigh);
    }

    [Fact]
    public void Claims_ReturnsDistinctClaimTypes()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim("custom-claim", "value1"),
            new Claim("custom-claim", "value2"),
            new Claim("another-claim", "value3")
        }));

        HttpClaimsAuthorisationContext context = new(user);

        IReadOnlyCollection<string> claims = context.Claims;

        Assert.Contains("custom-claim", claims);
        Assert.Contains("another-claim", claims);
        Assert.Equal(2, claims.Count);
    }
}
