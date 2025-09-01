using DfE.GIAP.Web.Features.Session.Abstractions;
using DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore;
using DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore.Command;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Session;
public sealed class AspNetCoreSessionCommandHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_SessionProvider_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectKeyResolver> keyResolver = new();
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializer = new();
        Func<AspNetCoreSessionCommandHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionCommandHandler<StubSessionObject>(null, keyResolver.Object, sessionObjectSerializer.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_SessionKeyResolver_Is_Null()
    {
        // Arrange
        Mock<IAspNetCoreSessionProvider> sessionProvider = new();
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializer = new();
        Func<AspNetCoreSessionCommandHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionCommandHandler<StubSessionObject>(sessionProvider.Object, null, sessionObjectSerializer.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Serializer_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectKeyResolver> keyResolver = new();
        Mock<IAspNetCoreSessionProvider> sessionProvider = new();
        Func<AspNetCoreSessionCommandHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionCommandHandler<StubSessionObject>(sessionProvider.Object, keyResolver.Object, null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void StoreInSession_Throws_When_Value_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectKeyResolver> keyResolver = new();
        Mock<IAspNetCoreSessionProvider> sessionProvider = new();
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializer = new();

        AspNetCoreSessionCommandHandler<StubSessionObject> sut = new(sessionProvider.Object, keyResolver.Object, sessionObjectSerializer.Object);

        Action act = () => sut.StoreInSession(null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void StoreInSession_ResolvesKey_And_Stores_Serialized_Value()
    {
        Mock<ISessionObjectKeyResolver> keyResolver = new();
        keyResolver
            .Setup(t => t.Resolve<StubSessionObject>())
            .Returns(It.IsAny<string>())
            .Verifiable();

        Mock<ISession> sessionMock = new();
        sessionMock
            .Setup((session) => session.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Verifiable();

        Mock<IAspNetCoreSessionProvider> sessionProviderMock = new();
        sessionProviderMock
            .Setup(provider => provider.GetSession())
            .Returns(sessionMock.Object)
            .Verifiable();

        const string stubSerialisedValue = "store_this";
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializer = new();
        sessionObjectSerializer
            .Setup(serializer => serializer.Serialize(It.IsAny<StubSessionObject>()))
            .Returns(stubSerialisedValue)
            .Verifiable();

        AspNetCoreSessionCommandHandler<StubSessionObject> sut = new(sessionProviderMock.Object, keyResolver.Object, sessionObjectSerializer.Object);

        StubSessionObject stubSessionObject = new();
        sut.StoreInSession(stubSessionObject);

        // Act Assert
        sessionMock.Verify(t => t.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        sessionProviderMock.Verify(t => t.GetSession(), Times.Once);
        keyResolver.Verify(t => t.Resolve<StubSessionObject>(), Times.Once);
        sessionObjectSerializer.Verify(t => t.Serialize(stubSessionObject), Times.Once);
    }

    public sealed class StubSessionObject { }
}
