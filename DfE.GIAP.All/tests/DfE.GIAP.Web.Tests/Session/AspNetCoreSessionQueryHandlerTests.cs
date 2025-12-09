using DfE.GIAP.Web.Session.Abstraction;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore.Query;
using DfE.GIAP.Web.Tests.Session.TestDoubles;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Session;
public sealed class AspNetCoreSessionQueryHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_SessionProvider_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectKeyResolver> keyResolverMock = ISessionObjectKeyResolverTestDoubles.Default();
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializerMock = ISessionObjectSerializerTestDoubles.Default<StubSessionObject>();

        Func<AspNetCoreSessionQueryHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionQueryHandler<StubSessionObject>(
                null,
                keyResolverMock.Object,
                sessionObjectSerializerMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_SessionKeyResolver_Is_Null()
    {
        // Arrange
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = new();
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializerMock = ISessionObjectSerializerTestDoubles.Default<StubSessionObject>();

        Func<AspNetCoreSessionQueryHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionQueryHandler<StubSessionObject>(
                sessionProviderMock.Object,
                null,
                sessionObjectSerializerMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Serializer_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectKeyResolver> keyResolverMock = ISessionObjectKeyResolverTestDoubles.Default();
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = new();
        Func<AspNetCoreSessionQueryHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionQueryHandler<StubSessionObject>(
                sessionProviderMock.Object,
                keyResolverMock.Object,
                null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Session_DoesNotContain_Key_Returns_Default_And_ValueDoesNotExist()
    {
        // Arrange
        const string stubSessionObjectKey = "key";
        Mock<ISessionObjectKeyResolver> keyResolverMock = ISessionObjectKeyResolverTestDoubles.MockFor<StubSessionObject>(stubSessionObjectKey);

        Mock<ISession> sessionMock = ISessionTestDoubles.MockForTryGetValue(key: stubSessionObjectKey, tryGetResult: false);
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = IAspNetCoreSessionProviderTestDoubles.MockWithSession(sessionMock.Object);
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializerMock = ISessionObjectSerializerTestDoubles.Default<StubSessionObject>();

        AspNetCoreSessionQueryHandler<StubSessionObject> sut = new(sessionProviderMock.Object, keyResolverMock.Object, sessionObjectSerializerMock.Object);

        // Act
        SessionQueryResponse<StubSessionObject> sessionQueryResponse = sut.Handle();

        // Assert
        Assert.Null(sessionQueryResponse.Value); // defaults
        Assert.False(sessionQueryResponse.HasValue);

        sessionProviderMock.Verify(t => t.GetSession(), Times.Once);
        keyResolverMock.Verify(t => t.Resolve<StubSessionObject>(), Times.Once);
    }

    [Fact]
    public void Session_Contains_Key_Returns_Deserialised_Object()
    {
        // Arrange
        const string stubSessionObjectKey = "test-key";
        Mock<ISessionObjectKeyResolver> keyResolverMock = ISessionObjectKeyResolverTestDoubles.MockFor<StubSessionObject>(resolvedKey: stubSessionObjectKey);

        Mock<ISession> sessionMock = ISessionTestDoubles.MockForTryGetValue(key: stubSessionObjectKey, true);
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = IAspNetCoreSessionProviderTestDoubles.MockWithSession(sessionMock.Object);

        StubSessionObject stubSessionObject = new();
        Mock<ISessionObjectSerializer<StubSessionObject>> serializer = ISessionObjectSerializerTestDoubles.MockDeserialize(stubSessionObject);
        
        AspNetCoreSessionQueryHandler<StubSessionObject> sut = new(sessionProviderMock.Object, keyResolverMock.Object, serializer.Object);

        // Act
        SessionQueryResponse<StubSessionObject> sessionQueryResponse = sut.Handle();

        // Assert
        Assert.Equal(stubSessionObject, sessionQueryResponse.Value);
        Assert.True(sessionQueryResponse.HasValue);

        keyResolverMock.Verify(t => t.Resolve<StubSessionObject>(), Times.Once);
        sessionProviderMock.Verify(t => t.GetSession(), Times.Once);
        serializer.Verify(t => t.Deserialize(stubSessionObjectKey), Times.Once);
    }

    public class StubSessionObject { }
}
