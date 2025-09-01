using DfE.GIAP.Web.Features.Session;
using DfE.GIAP.Web.Features.Session.Infrastructure.KeyResolver;
using DfE.GIAP.Web.Features.Session.Infrastructure.Provider;
using DfE.GIAP.Web.Features.Session.Query;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Session;
public sealed class AspNetCoreSessionQueryHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_SessionProvider_Is_Null()
    {
        Mock<ISessionObjectKeyResolver> keyResolver = new();
        Func<AspNetCoreSessionQueryHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionQueryHandler<StubSessionObject>(null, keyResolver.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_SessionKeyResolver_Is_Null()
    {
        Mock<IAspNetCoreSessionProvider> sessionProvider = new();
        Func<AspNetCoreSessionQueryHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionQueryHandler<StubSessionObject>(sessionProvider.Object, null);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Session_DoesNotContain_Key_Returns_Default_And_ValueDoesNotExist()
    {
        const string stubSessionObjectKey = "key";
        Mock<ISessionObjectKeyResolver> keyResolver = new();
        keyResolver.Setup(t => t.Resolve<StubSessionObject>()).Returns(stubSessionObjectKey);

        Mock<ISession> sessionMock = new();
        sessionMock
              .Setup(s => s.TryGetValue(stubSessionObjectKey, out It.Ref<byte[]>.IsAny!))
              .Returns(false);

        Mock<IAspNetCoreSessionProvider> sessionProviderMock = new();
        sessionProviderMock.Setup(t => t.GetSession()).Returns(sessionMock.Object);

        AspNetCoreSessionQueryHandler<StubSessionObject> sut = new(sessionProviderMock.Object, keyResolver.Object);

        // Act
        SessionQueryResponse<StubSessionObject> sessionQueryResponse = sut.GetSessionObject();

        // Assert
        Assert.Null(sessionQueryResponse.Value); // defaults
        Assert.False(sessionQueryResponse.HasValue);
    }

    private sealed class StubSessionObject { }
}
