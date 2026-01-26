using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Logging.Middleware;
using DfE.GIAP.Web.Providers.Session;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Logging.Middleware;

public class SessionCorrelationIdContextMiddlewareTests
{
    private readonly Mock<ISessionProvider> _sessionProviderMock;
    private readonly DefaultHttpContext _httpContext;
    private bool _nextCalled;

    public SessionCorrelationIdContextMiddlewareTests()
    {
        _sessionProviderMock = new Mock<ISessionProvider>();
        _httpContext = new DefaultHttpContext();
        _nextCalled = false;
    }

    private SessionCorrelationIdMiddleware CreateMiddleware()
    {
        RequestDelegate next = (ctx) =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        };

        return new SessionCorrelationIdMiddleware(next);
    }

    [Fact]
    public async Task InvokeAsync_WhenCorrelationId_Exists_DoesNotCallSessionProvider()
    {
        // Arrange
        string existingId = Guid.NewGuid().ToString();
        _sessionProviderMock
            .Setup(sp => sp.GetSessionValueOrDefault<string>(SessionKeys.CorrelationId))
            .Returns(existingId);

        SessionCorrelationIdMiddleware middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(_httpContext, _sessionProviderMock.Object);

        // Assert
        _sessionProviderMock.Verify(
            sp => sp.SetSessionValue<string>(SessionKeys.CorrelationId, It.IsAny<string>()),
            Times.Never);

        Assert.True(_nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenCorrelationId_IsNull_CallsSessionProvider()
    {
        // Arrange
        _sessionProviderMock
            .Setup(sp => sp.GetSessionValueOrDefault<string>(SessionKeys.CorrelationId))
            .Returns<string?>(null!);

        SessionCorrelationIdMiddleware middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(_httpContext, _sessionProviderMock.Object);

        // Assert
        _sessionProviderMock.Verify(
            sp => sp.SetSessionValue<string>(
                SessionKeys.CorrelationId,
                It.IsAny<string>()),
            Times.Once);

        Assert.True(_nextCalled);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task InvokeAsync_WhenCorrelationId_IsEmptyOrWhitespace_CallsSessionProvider(string emptyValue)
    {
        // Arrange
        _sessionProviderMock
            .Setup(sp => sp.GetSessionValueOrDefault<string>(SessionKeys.CorrelationId))
            .Returns(emptyValue);

        SessionCorrelationIdMiddleware middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(_httpContext, _sessionProviderMock.Object);

        // Assert
        _sessionProviderMock.Verify(
            sp => sp.SetSessionValue<string>(
                SessionKeys.CorrelationId,
                It.IsAny<string>()),
            Times.Once);

        Assert.True(_nextCalled);
    }
}
