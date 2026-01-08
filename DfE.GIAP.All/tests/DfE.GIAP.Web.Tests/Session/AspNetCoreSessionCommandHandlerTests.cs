using System.Text;
using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Infrastructure.AspNetCore;
using DfE.GIAP.Web.Tests.Session.TestDoubles;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Session;
public sealed class AspNetCoreSessionCommandHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_SessionProvider_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectKeyResolver> keyResolverMock = ISessionObjectKeyResolverTestDoubles.Default();
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializerMock = ISessionObjectSerializerTestDoubles.Default<StubSessionObject>();

        Func<AspNetCoreSessionCommandHandler<StubSessionObject>> construct = ()
            => new AspNetCoreSessionCommandHandler<StubSessionObject>(null, keyResolverMock.Object, sessionObjectSerializerMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_SessionKeyResolver_Is_Null()
    {
        // Arrange
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = IAspNetCoreSessionProviderTestDoubles.Default();
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializerMock = ISessionObjectSerializerTestDoubles.Default<StubSessionObject>();

        Func<AspNetCoreSessionCommandHandler<StubSessionObject>> construct = ()
            => new AspNetCoreSessionCommandHandler<StubSessionObject>(sessionProviderMock.Object, null, sessionObjectSerializerMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Serializer_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectKeyResolver> keyResolverMock = ISessionObjectKeyResolverTestDoubles.Default();
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = IAspNetCoreSessionProviderTestDoubles.Default();

        Func<AspNetCoreSessionCommandHandler<StubSessionObject>> construct = ()
            => new AspNetCoreSessionCommandHandler<StubSessionObject>(sessionProviderMock.Object, keyResolverMock.Object, null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void StoreInSession_Throws_When_Value_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectKeyResolver> keyResolverMock = ISessionObjectKeyResolverTestDoubles.Default();
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = IAspNetCoreSessionProviderTestDoubles.Default();
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializerMock = ISessionObjectSerializerTestDoubles.Default<StubSessionObject>();

        AspNetCoreSessionCommandHandler<StubSessionObject> sut = new(sessionProviderMock.Object, keyResolverMock.Object, sessionObjectSerializerMock.Object);

        Action act = () => sut.StoreInSession(value: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void StoreInSession_ResolvesKey_And_Stores_Serialized_Value()
    {
        const string sessionObjectAccessKey = "query-key";
        Mock<ISessionObjectKeyResolver> keyResolver = ISessionObjectKeyResolverTestDoubles.MockFor<StubSessionObject>(resolvedKey: sessionObjectAccessKey);

        Mock<ISession> sessionMock = ISessionTestDoubles.MockForSet();
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = IAspNetCoreSessionProviderTestDoubles.MockWithSession(sessionMock.Object);

        const string stubSerialisedValue = "store_this";
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializer = ISessionObjectSerializerTestDoubles.MockSerialize<StubSessionObject>(stubSerialisedValue);

        AspNetCoreSessionCommandHandler<StubSessionObject> sut = new(sessionProviderMock.Object, keyResolver.Object, sessionObjectSerializer.Object);

        StubSessionObject stubSessionObject = new();
        sut.StoreInSession(stubSessionObject);

        // Act Assert
        sessionMock.Verify(t => t.Set(sessionObjectAccessKey, Encoding.UTF8.GetBytes(stubSerialisedValue)), Times.Once);
        sessionProviderMock.Verify(t => t.GetSession(), Times.Once);
        keyResolver.Verify(t => t.Resolve<StubSessionObject>(), Times.Once);
        sessionObjectSerializer.Verify(t => t.Serialize(stubSessionObject), Times.Once);
    }

    public sealed class StubSessionObject { }
}
