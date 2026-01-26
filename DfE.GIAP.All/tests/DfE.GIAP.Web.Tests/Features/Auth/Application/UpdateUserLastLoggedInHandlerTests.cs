using System.Security.Claims;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLoggedIn;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Auth.Application;

public class UpdateUserLastLoggedInHandlerTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WithNullUseCase()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new UpdateUserLastLoggedInHandler(null!));
    }

    [Fact]
    public async Task HandleAsync_ShouldCallUpdateLastLoggedInUseCase()
    {
        // Arrange
        Mock<IUseCaseRequestOnly<UpdateLastLoggedInRequest>> mockUseCase = new();
        UpdateUserLastLoggedInHandler sut = new(mockUseCase.Object);

        ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.UserId, "user-123")
        }));

        TokenAuthorisationContext context = new(
            principal: principal);

        // Act
        await sut.HandleAsync(context);

        // Assert
        mockUseCase.Verify(x => x.HandleRequestAsync(
            It.Is<UpdateLastLoggedInRequest>(r => r.UserId == "user-123")),
            Times.Once);
    }
}
