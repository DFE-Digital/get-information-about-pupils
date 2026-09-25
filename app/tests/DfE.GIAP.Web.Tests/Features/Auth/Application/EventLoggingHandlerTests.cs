using System.Security.Claims;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Auth.Application;

public class EventLoggingHandlerTests
{
    [Fact]
    public void Constructor_NullEventLogger_Throws_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new EventLoggingHandler(null!));
    }

    [Fact]
    public void Constructor_ValidEventLogger_Succeeds()
    {
        // Arrange
        Mock<IEventLogger> mockLogger = new();

        // Act
        EventLoggingHandler handler = new(mockLogger.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task HandleAsync_CallLogSignin_WithExpectedValues()
    {
        // Arrange
        Mock<IEventLogger> mockLogger = new();
        EventLoggingHandler handler = new(mockLogger.Object);

        ClaimsPrincipal principle = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.UserId, "user123"),
            new Claim(AuthClaimTypes.SessionId, "session456"),
            new Claim(AuthClaimTypes.UniqueReferenceNumber, "urn789"),
            new Claim(AuthClaimTypes.OrganisationName, "TestOrg"),
            new Claim(AuthClaimTypes.OrganisationCategoryId, "Cat001"),
        }));

        TokenAuthorisationContext context = new(principle);

        // Act
        await handler.HandleAsync(context);

        // Assert
        mockLogger.Verify(
            l => l.LogSignin("user123", "session456", "urn789", "TestOrg", "Cat001"),
            Times.Once);
    }
}
