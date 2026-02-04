using System.Text;
using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Infrastructure;
using DfE.GIAP.Web.Shared.Session.Infrastructure.Command;
using Microsoft.AspNetCore.Http;

namespace DfE.GIAP.Web.Tests.Shared.Session;
public sealed class AspNetCoreSessionCommandHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_SessionProvider_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializerMock = ISessionObjectSerializerTestDoubles.Default<StubSessionObject>();

        Func<AspNetCoreSessionCommandHandler<StubSessionObject>> construct = ()
            => new AspNetCoreSessionCommandHandler<StubSessionObject>(null, sessionObjectSerializerMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Serializer_Is_Null()
    {
        // Arrange
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = IAspNetCoreSessionProviderTestDoubles.Default();

        Func<AspNetCoreSessionCommandHandler<StubSessionObject>> construct = ()
            => new AspNetCoreSessionCommandHandler<StubSessionObject>(sessionProviderMock.Object, null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void StoreInSession_Throws_When_Value_Is_Null()
    {
        // Arrange
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = IAspNetCoreSessionProviderTestDoubles.Default();
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializerMock = ISessionObjectSerializerTestDoubles.Default<StubSessionObject>();

        AspNetCoreSessionCommandHandler<StubSessionObject> sut = new(sessionProviderMock.Object, sessionObjectSerializerMock.Object);

        Action act = () => sut.StoreInSession(new SessionCacheKey("key"), value: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void StoreInSession_Stores_Serialized_Value()
    {
        Mock<ISession> sessionMock = ISessionTestDoubles.MockForSet();
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = IAspNetCoreSessionProviderTestDoubles.MockWithSession(sessionMock.Object);

        const string stubSerialisedValue = "store_this";
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializer = ISessionObjectSerializerTestDoubles.MockSerialize<StubSessionObject>(stubSerialisedValue);

        AspNetCoreSessionCommandHandler<StubSessionObject> sut = new(sessionProviderMock.Object, sessionObjectSerializer.Object);

        StubSessionObject stubSessionObject = new();
        sut.StoreInSession(new SessionCacheKey("key"), stubSessionObject);

        // Act Assert
        sessionMock.Verify(t => t.Set("key", Encoding.UTF8.GetBytes(stubSerialisedValue)), Times.Once);
        sessionProviderMock.Verify(t => t.GetSession(), Times.Once);
        sessionObjectSerializer.Verify(t => t.Serialize(stubSessionObject), Times.Once);
    }

    public sealed class StubSessionObject { }
}
