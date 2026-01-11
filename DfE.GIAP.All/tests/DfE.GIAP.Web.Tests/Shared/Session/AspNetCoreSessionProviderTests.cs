using DfE.GIAP.Web.Shared.Session.Infrastructure.AspNetCore;
using DfE.GIAP.Web.Tests.Shared.Http;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Shared.Session;
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
        Mock<IHttpContextAccessor> httpContextAccessorMock = IHttpContextAccessorTestDoubles.WithHttpContext(null);

        // Act
        AspNetCoreSessionProvider sut = new(httpContextAccessorMock.Object);

        // Assert
        InvalidOperationException noContextException = Assert.Throws<InvalidOperationException>(() => sut.GetSession());
        Assert.Contains("HttpContext", noContextException.Message);
    }

    [Fact]
    public void GetSession_Throws_When_Session_Is_Null()
    {
        // Arrange
        HttpContext httpContextStub = HttpContextTestDoubles.WithSession(null!);
        Mock<IHttpContextAccessor> httpContextAccessorMock = IHttpContextAccessorTestDoubles.WithHttpContext(httpContextStub);

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
        Mock<ISession> sessionMock = new();
        HttpContext httpContextStub = HttpContextTestDoubles.WithSession(sessionMock.Object);
        Mock<IHttpContextAccessor> httpContextAccessorMock = IHttpContextAccessorTestDoubles.WithHttpContext(httpContextStub);


        // Act
        AspNetCoreSessionProvider sut = new(httpContextAccessorMock.Object);

        // Assert
        ISession session = sut.GetSession();
        Assert.NotNull(session);
        Assert.Same(sessionMock.Object, session);
    }
}
