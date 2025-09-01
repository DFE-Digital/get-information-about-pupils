using DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Session;
public sealed class AspNetCoreSessionProviderTests
{
    [Fact]
    public void Constructor_Throws_When_HttpContextAccessor_Is_Null()
    {
        //Arrange
        Func<AspNetCoreSessionProvider> construct = () => new(null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void GetSession_Throws_When_HttpContext_Is_Null()
    {
        // Arrange
        Mock<IHttpContextAccessor> httpContextAccessorMock = new();
        httpContextAccessorMock.SetupGet(t => t.HttpContext).Returns(() => null);

        // Act
        AspNetCoreSessionProvider sut = new(httpContextAccessorMock.Object);

        // Assert
        InvalidOperationException noContextException =  Assert.Throws<InvalidOperationException>(() => sut.GetSession());
        Assert.Contains("HttpContext", noContextException.Message);
    }

    [Fact]
    public void GetSession_Throws_When_Session_Is_Null()
    {
        // Arrange
        Mock<IHttpContextAccessor> httpContextAccessorMock = new();
        DefaultHttpContext httpContextStub = new()
        {
            Session = null!
        };
        httpContextAccessorMock.SetupGet(t => t.HttpContext).Returns(httpContextStub);

        // Act
        AspNetCoreSessionProvider sut = new(httpContextAccessorMock.Object);

        // Assert
        InvalidOperationException noContextException = Assert.Throws<InvalidOperationException>(() => sut.GetSession());
        Assert.Contains("Session", noContextException.Message);
    }

    [Fact]
    public void GetSession_Returns_Session()
    {
        // Arrange
        Mock<IHttpContextAccessor> httpContextAccessorMock = new();
        Mock<ISession> sessionMock = new();
        DefaultHttpContext httpContextStub = new()
        {
            Session = sessionMock.Object
        };
        httpContextAccessorMock.SetupGet(t => t.HttpContext).Returns(httpContextStub);

        // Act
        AspNetCoreSessionProvider sut = new(httpContextAccessorMock.Object);

        // Assert
        ISession session = sut.GetSession();
        Assert.NotNull(session);
        Assert.Same(sessionMock.Object, session);
    }
}
