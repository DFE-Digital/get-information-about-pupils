using System.Security.Claims;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;
using DfE.GIAP.Web.Providers.Session;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Auth.Application;

public class SetUnreadNewsStatusHandlerTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WithNullUseCase()
    {
        // Arrange
        ISessionProvider sessionProvider = Mock.Of<ISessionProvider>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new SetUnreadNewsStatusHandler(null!, sessionProvider));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WithNullSessionProvider()
    {
        // Arrange
        Mock<IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse>> useCase = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new SetUnreadNewsStatusHandler(useCase.Object, null!));
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public async Task HandleAsync_ShouldSetSessionValueBasedOnHasUpdates(bool hasUpdates, int expectedCalls)
    {
        // Arrange
        Mock<IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse>> mockUseCase = new();
        Mock<ISessionProvider> mockSessionProvider = new();

        mockUseCase.Setup(x => x.HandleRequestAsync(It.IsAny<GetUnreadUserNewsRequest>()))
            .ReturnsAsync(new GetUnreadUserNewsResponse(HasUpdates: hasUpdates));

        SetUnreadNewsStatusHandler sut = new(
            mockUseCase.Object, mockSessionProvider.Object);

        ClaimsPrincipal principal = new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.UserId, "user-123")
        }));

        TokenAuthorisationContext context = new(principal);

        // Act
        await sut.HandleAsync(context);

        // Assert
        mockSessionProvider.Verify(x =>
            x.SetSessionValue(SessionKeys.ShowNewsBannerKey, true), Times.Exactly(expectedCalls));
    }
}
