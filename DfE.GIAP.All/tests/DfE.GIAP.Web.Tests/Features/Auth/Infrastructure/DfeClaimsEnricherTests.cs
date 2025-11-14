using System.Security.Claims;
using DfE.GIAP.Web.Features.Auth.Application;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Features.Auth.Infrastructure;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Config;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Auth.Infrastructure;

public class DfeClaimsEnricherTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WithNullApiClient()
    {
        // Arrange
        IOptions<DsiOptions> options = Options.Create(new DsiOptions());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DfeClaimsEnricher(null!, options));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WithNullOptionsValue()
    {
        // Arrange
        Mock<IDfeSignInApiClient> mockApiClient = new();
        IOptions<DsiOptions> options = Options.Create<DsiOptions>(null!);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DfeClaimsEnricher(mockApiClient.Object, options));
    }

    [Theory]
    [InlineData(AuthClaimTypes.UserId, "user-abc")]
    [InlineData(ClaimTypes.Email, "user@example.com")]
    [InlineData(ClaimTypes.Role, AuthRoles.Admin)]
    [InlineData(ClaimTypes.Role, AuthRoles.User)]
    [InlineData(AuthClaimTypes.OrganisationId, "org-xyz")]
    [InlineData(AuthClaimTypes.OrganisationName, "Test Org")]
    [InlineData(AuthClaimTypes.OrganisationCategoryId, "cat-1")]
    [InlineData(AuthClaimTypes.OrganisationEstablishmentTypeId, "type-1")]
    [InlineData(AuthClaimTypes.OrganisationLowAge, "5")]
    [InlineData(AuthClaimTypes.OrganisationHighAge, "18")]
    [InlineData(AuthClaimTypes.EstablishmentNumber, "123456")]
    [InlineData(AuthClaimTypes.LocalAuthorityNumber, "LA001")]
    [InlineData(AuthClaimTypes.UniqueReferenceNumber, "URN001")]
    [InlineData(AuthClaimTypes.UniqueIdentifier, "UID001")]
    [InlineData(AuthClaimTypes.UKProviderReferenceNumber, "UKPRN001")]
    public async Task EnrichAsync_ShouldContain_ExpectedClaim(string claimType, string expectedValue)
    {
        // Arrange
        string userId = "user-abc";
        string email = "user@example.com";
        string orgId = "org-xyz";

        ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", userId),
            new Claim("email", email),
            new Claim("organisation", $"{{\"id\":\"{orgId}\"}}")
        }));

        Mock<IDfeSignInApiClient> mockApiClient = new();
        IOptions<DsiOptions> options = Options.Create(new DsiOptions { ServiceId = "service-123" });

        mockApiClient.Setup(x => x.GetUserInfo("service-123", orgId, userId))
            .ReturnsAsync(new UserAccess
            {
                Roles = new List<UserRole>
                {
                    new UserRole { Code = AuthRoles.Admin },
                    new UserRole { Code = AuthRoles.User }
                }
            });

        mockApiClient.Setup(x => x.GetUserOrganisation(userId, orgId))
            .ReturnsAsync(new Organisation
            {
                Id = orgId,
                Name = "Test Org",
                Category = new OrganisationCategory { Id = "cat-1" },
                EstablishmentType = new EstablishmentType { Id = "type-1" },
                StatutoryLowAge = "5",
                StatutoryHighAge = "18",
                EstablishmentNumber = "123456",
                LocalAuthority = new LocalAuthority { Code = "LA001" },
                UniqueReferenceNumber = "URN001",
                UniqueIdentifier = "UID001",
                UKProviderReferenceNumber = "UKPRN001"
            });

        DfeClaimsEnricher sut = new(mockApiClient.Object, options);

        // Act
        ClaimsPrincipal enrichedPrincipal = await sut.EnrichAsync(principal);

        // Assert
        ClaimsIdentity identity = (ClaimsIdentity)enrichedPrincipal.Identity!;
        List<Claim> claims = new(identity.Claims);

        Assert.Contains(claims, c => c.Type == claimType && c.Value == expectedValue);
    }

    [Theory]
    [InlineData(AuthClaimTypes.OrganisationId, "")]
    [InlineData(AuthClaimTypes.OrganisationName, "")]
    [InlineData(AuthClaimTypes.OrganisationCategoryId, "")]
    [InlineData(AuthClaimTypes.OrganisationEstablishmentTypeId, "")]
    [InlineData(AuthClaimTypes.OrganisationLowAge, "0")]
    [InlineData(AuthClaimTypes.OrganisationHighAge, "0")]
    [InlineData(AuthClaimTypes.EstablishmentNumber, "")]
    [InlineData(AuthClaimTypes.LocalAuthorityNumber, "")]
    [InlineData(AuthClaimTypes.UniqueReferenceNumber, "")]
    [InlineData(AuthClaimTypes.UniqueIdentifier, "")]
    [InlineData(AuthClaimTypes.UKProviderReferenceNumber, "")]
    public async Task EnrichAsync_WhenOrganisationIsNull_ShouldSetExpectedClaimValue(string claimType, string expectedValue)
    {
        // Arrange
        string userId = "user-abc";
        string email = "user@example.com";
        string orgId = "org-xyz";

        ClaimsPrincipal principal = new(new ClaimsIdentity(new[]
        {
            new Claim("sub", userId),
            new Claim("email", email),
            new Claim("organisation", $"{{\"id\":\"{orgId}\"}}")
        }));

        Mock<IDfeSignInApiClient> mockApiClient = new();
        IOptions<DsiOptions> options = Options.Create(new DsiOptions { ServiceId = "service-123" });

        mockApiClient.Setup(x => x.GetUserInfo("service-123", orgId, userId))
            .ReturnsAsync(new UserAccess { Roles = new List<UserRole>() });

        mockApiClient.Setup(x => x.GetUserOrganisation(userId, orgId))
            .ReturnsAsync((Organisation?)null);

        DfeClaimsEnricher sut = new(mockApiClient.Object, options);

        // Act
        ClaimsPrincipal enrichedPrincipal = await sut.EnrichAsync(principal);

        // Assert
        ClaimsIdentity identity = (ClaimsIdentity)enrichedPrincipal.Identity!;
        Claim? claim = identity.FindFirst(claimType);

        Assert.NotNull(claim);
        Assert.Equal(expectedValue, claim!.Value);
    }


    [Fact]
    public async Task EnrichAsync_WhenUserAccessIsNull_ShouldNotAddRoleClaims()
    {
        // Arrange
        string userId = "user-abc";
        string email = "user@example.com";
        string orgId = "org-xyz";

        ClaimsPrincipal principal = new(new ClaimsIdentity(new[]
        {
            new Claim("sub", userId),
            new Claim("email", email),
            new Claim("organisation", $"{{\"id\":\"{orgId}\"}}")
        }));

        Mock<IDfeSignInApiClient> mockApiClient = new();
        IOptions<DsiOptions> options = Options.Create(new DsiOptions { ServiceId = "service-123" });

        mockApiClient.Setup(x => x.GetUserInfo("service-123", orgId, userId))
            .ReturnsAsync((UserAccess?)null);

        mockApiClient.Setup(x => x.GetUserOrganisation(userId, orgId))
            .ReturnsAsync(new Organisation());

        DfeClaimsEnricher enricher = new(mockApiClient.Object, options);

        // Act
        ClaimsPrincipal enrichedPrincipal = await enricher.EnrichAsync(principal);

        // Assert
        ClaimsIdentity identity = (ClaimsIdentity)enrichedPrincipal.Identity!;
        List<Claim> claims = new(identity.Claims);

        Assert.DoesNotContain(claims, c => c.Type == ClaimTypes.Role);
    }
}
