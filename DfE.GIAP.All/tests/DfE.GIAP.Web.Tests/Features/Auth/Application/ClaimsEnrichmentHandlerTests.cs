using System.Security.Claims;
using DfE.GIAP.Web.Features.Auth.Application;
using DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Auth.Application;

public class ClaimsEnrichmentHandlerTests
{
    [Fact]
    public void Constructor_ThrowArgumentNullException_WithNullEnricher()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ClaimsEnrichmentHandler(null!));
    }

    [Fact]
    public async Task HandleAsync_ShouldReplacePrincipalWithEnrichedPrincipal()
    {
        // Arrange
        Mock<IClaimsEnricher> mockEnricher = new();

        ClaimsPrincipal originalPrincipal = new(new ClaimsIdentity(new[]
        {
            new Claim("custom", "original")
        }));

        ClaimsPrincipal enrichedPrincipal = new(new ClaimsIdentity(new[]
        {
            new Claim("custom", "enriched")
        }));

        mockEnricher.Setup(x => x.EnrichAsync(originalPrincipal))
            .ReturnsAsync(enrichedPrincipal);

        ClaimsEnrichmentHandler sut = new(mockEnricher.Object);

        TokenAuthorisationContext context = new(originalPrincipal);

        // Act
        await sut.HandleAsync(context);

        // Assert
        mockEnricher.Verify(x => x.EnrichAsync(originalPrincipal), Times.Once);
        Assert.Equal("enriched", context.Principal.FindFirst("custom")?.Value);
    }
}
