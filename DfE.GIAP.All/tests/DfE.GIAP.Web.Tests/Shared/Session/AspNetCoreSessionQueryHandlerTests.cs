using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Abstraction.Query;
using DfE.GIAP.Web.Shared.Session.Infrastructure;
using DfE.GIAP.Web.Shared.Session.Infrastructure.Query;
using DfE.GIAP.Web.Tests.Shared.Session.TestDoubles;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Shared.Session;
public sealed class AspNetCoreSessionQueryHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_SessionProvider_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializerMock = ISessionObjectSerializerTestDoubles.Default<StubSessionObject>();

        Func<AspNetCoreSessionQueryHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionQueryHandler<StubSessionObject>(
                null,
                sessionObjectSerializerMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Serializer_Is_Null()
    {
        // Arrange
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = new();
        Func<AspNetCoreSessionQueryHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionQueryHandler<StubSessionObject>(
                sessionProviderMock.Object,
                null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Session_DoesNotContain_Key_Returns_Default_And_ValueDoesNotExist()
    {
        // Arrange
        const string stubSessionObjectKey = "key";

        Mock<ISession> sessionMock = ISessionTestDoubles.MockForTryGetValue(key: stubSessionObjectKey, tryGetResult: false);
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = IAspNetCoreSessionProviderTestDoubles.MockWithSession(sessionMock.Object);
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializerMock = ISessionObjectSerializerTestDoubles.Default<StubSessionObject>();

        AspNetCoreSessionQueryHandler<StubSessionObject> sut = new(sessionProviderMock.Object, sessionObjectSerializerMock.Object);

        // Act
        SessionQueryResponse<StubSessionObject> sessionQueryResponse = sut.Handle(new SessionCacheKey("key"));

        // Assert
        Assert.Null(sessionQueryResponse.Value); // defaults
        Assert.False(sessionQueryResponse.HasValue);

        sessionProviderMock.Verify(t => t.GetSession(), Times.Once);

        sessionMock.Verify(
              t => t.TryGetValue(
                  It.Is<string>(k => k == stubSessionObjectKey),
                  out It.Ref<byte[]>.IsAny!),
              Times.Once);
    }

    [Fact]
    public void Session_Contains_Key_Returns_Deserialised_Object()
    {
        // Arrange
        const string stubSessionObjectKey = "test-key";
        
        Mock<ISession> sessionMock = ISessionTestDoubles.MockForTryGetValue(key: stubSessionObjectKey, true);
        Mock<IAspNetCoreSessionProvider> sessionProviderMock = IAspNetCoreSessionProviderTestDoubles.MockWithSession(sessionMock.Object);

        StubSessionObject stubSessionObject = new();
        Mock<ISessionObjectSerializer<StubSessionObject>> serializer = ISessionObjectSerializerTestDoubles.MockDeserialize(stubSessionObject);

        AspNetCoreSessionQueryHandler<StubSessionObject> sut = new(sessionProviderMock.Object, serializer.Object);

        // Act
        SessionQueryResponse<StubSessionObject> sessionQueryResponse = sut.Handle(new SessionCacheKey(stubSessionObjectKey));

        // Assert
        Assert.Equal(stubSessionObject, sessionQueryResponse.Value);
        Assert.True(sessionQueryResponse.HasValue);

        sessionProviderMock.Verify(t => t.GetSession(), Times.Once);

        sessionMock.Verify(t => t.TryGetValue(
                It.Is<string>((key) => key == stubSessionObjectKey),
                out It.Ref<byte[]>.IsAny!),
                    Times.Once);

        serializer.Verify(t => t.Deserialize(stubSessionObjectKey), Times.Once);
    }

    public class StubSessionObject { }
}
