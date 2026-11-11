using System.Security.Claims;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Helpers;
using static DfE.GIAP.Web.Constants.DsiKeys;

namespace DfE.GIAP.Web.Tests.Helpers;

public sealed class HttpClaimsAuthorisationContextTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenUserIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HttpClaimsAuthorisationContext(null!));
    }

    [Fact]
    public void IsAdminUser_ReturnsTrue_WhenAdminClaimPresent()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, AuthRoles.Admin)
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.True(context.IsAdminUser);
    }

    [Fact]
    public void IsAdminUser_ReturnsFalse_WhenAdminClaimNotPresent()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, AuthRoles.Approver)
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.False(context.IsAdminUser);
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
    public void IsEstablishment_ReturnsTrue_WhenEstablishmentClaimPresent()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationCategoryId, OrganisationCategory.Establishment)
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.True(context.IsEstablishment);
    }

    [Fact]
    public void IsLAUser_ReturnsTrue_WhenLocalAuthorityClaimPresent()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationCategoryId, OrganisationCategory.LocalAuthority)
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.True(context.IsLAUser);
    }

    [Fact]
    public void IsMatUser_ReturnsTrue_WhenMultiAcademyTrustClaimPresent()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationCategoryId, OrganisationCategory.MultiAcademyTrust)
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.True(context.IsMatUser);
    }

    [Fact]
    public void IsSatUser_ReturnsTrue_WhenSingleAcademyTrustClaimPresent()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationCategoryId, OrganisationCategory.SingleAcademyTrust)
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.True(context.IsSatUser);
    }

    [Fact]
    public void AnyAgeUser_ReturnsTrue_WhenOrganisationAllAgesClaimsZero()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationHighAge, "0"),
            new Claim(AuthClaimTypes.OrganisationLowAge, "0")
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.True(context.AnyAgeUser);
    }

    [Fact]
    public void AnyAgeUser_ReturnsFalse_WhenOrganisationAllAgesClaimsNotZero()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.OrganisationHighAge, "1"),
            new Claim(AuthClaimTypes.OrganisationLowAge, "1")
        }));

        HttpClaimsAuthorisationContext context = new(user);

        Assert.False(context.AnyAgeUser);
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
