using System.Security.Claims;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Logging;
using DfE.GIAP.Web.Providers.Session;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Logging;

public class TraceLogFactoryTests
{
    private readonly Mock<ISessionProvider> _sessionProviderMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly TraceLogFactory _factory;

    public TraceLogFactoryTests()
    {
        _sessionProviderMock = new Mock<ISessionProvider>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _factory = new TraceLogFactory(_sessionProviderMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public void Create_ShouldPopulatePayload_WithExpectedValues()
    {
        // Arrange
        string correlationId = "corr-1";
        _sessionProviderMock
            .Setup(s => s.GetSessionValueOrDefault<string>(SessionKeys.CorrelationId))
            .Returns(correlationId);

        Claim[] claims = new[]
        {
            new Claim(CustomClaimTypes.UserId, "user-1"),
            new Claim(CustomClaimTypes.SessionId, "sess-1")
        };
        ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);

        DefaultHttpContext httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        TracePayloadOptions options = new TracePayloadOptions
        {
            Message = "Test message",
            Level = LogLevel.Information,
            Category = "TestCategory",
            Source = "UnitTest",
            Context = new()
        };

        // Act
        Log<TracePayload> log = _factory.Create(options);

        // Assert
        Assert.NotNull(log);
        Assert.NotNull(log.Payload);
        Assert.Equal("Test message", log.Payload.Message);
        Assert.Equal(correlationId, log.Payload.CorrelationId);
        Assert.Equal("user-1", log.Payload.UserID);
        Assert.Equal("sess-1", log.Payload.SessionId);
        Assert.Equal(LogLevel.Information, log.Payload.Level);
        Assert.Equal("TestCategory", log.Payload.Category);
        Assert.Equal("UnitTest", log.Payload.Source);
        Assert.Equal(new(), log.Payload.Context);
    }

    [Fact]
    public void Create_ShouldHandleNullUser_AndNullMessage()
    {
        // Arrange
        _sessionProviderMock
            .Setup(s => s.GetSessionValueOrDefault<string>(SessionKeys.CorrelationId))
            .Returns("corr-1");

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

        TracePayloadOptions options = new TracePayloadOptions
        {
            Message = null,
            Level = LogLevel.Information
        };

        // Act
        Log<TracePayload> log = _factory.Create(options);

        // Assert
        Assert.NotNull(log.Payload);
        Assert.Equal(string.Empty, log.Payload.Message); // null becomes empty string
        Assert.Equal("corr-1", log.Payload.CorrelationId);
        Assert.Null(log.Payload.UserID);
        Assert.Null(log.Payload.SessionId);
        Assert.Equal(LogLevel.Information, log.Payload.Level);
    }
}
