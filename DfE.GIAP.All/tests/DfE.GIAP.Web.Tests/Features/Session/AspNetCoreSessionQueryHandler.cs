using System.Text;
using DfE.GIAP.Web.Features.Session.Abstractions;
using DfE.GIAP.Web.Features.Session.Abstractions.Query;
using DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore;
using DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore.Query;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Session;
public sealed class AspNetCoreSessionQueryHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_SessionProvider_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectKeyResolver> keyResolver = new();
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializer = new();
        Func<AspNetCoreSessionQueryHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionQueryHandler<StubSessionObject>(null, keyResolver.Object, sessionObjectSerializer.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_SessionKeyResolver_Is_Null()
    {
        // Arrange
        Mock<IAspNetCoreSessionProvider> sessionProvider = new();
        Mock<ISessionObjectSerializer<StubSessionObject>> sessionObjectSerializer = new();
        Func<AspNetCoreSessionQueryHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionQueryHandler<StubSessionObject>(sessionProvider.Object, null, sessionObjectSerializer.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Serializer_Is_Null()
    {
        // Arrange
        Mock<ISessionObjectKeyResolver> keyResolver = new();
        Mock<IAspNetCoreSessionProvider> sessionProvider = new();
        Func<AspNetCoreSessionQueryHandler<StubSessionObject>> construct =
            () => new AspNetCoreSessionQueryHandler<StubSessionObject>(sessionProvider.Object, keyResolver.Object, null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Session_DoesNotContain_Key_Returns_Default_And_ValueDoesNotExist()
    {
        // Arrange
        const string stubSessionObjectKey = "key";
        Mock<ISessionObjectKeyResolver> keyResolver = new();
        keyResolver.Setup(t => t.Resolve<StubSessionObject>()).Returns(stubSessionObjectKey);

        Mock<ISession> sessionMock = new();
        sessionMock
            .Setup(s => s.TryGetValue(stubSessionObjectKey, out It.Ref<byte[]>.IsAny!))
            .Returns(false);

        Mock<IAspNetCoreSessionProvider> sessionProviderMock = new();
        sessionProviderMock.Setup(t => t.GetSession()).Returns(sessionMock.Object);

        Mock<ISessionObjectSerializer<StubSessionObject>> serializer = new();

        AspNetCoreSessionQueryHandler<StubSessionObject> sut = new(sessionProviderMock.Object, keyResolver.Object, serializer.Object);

        // Act
        SessionQueryResponse<StubSessionObject> sessionQueryResponse = sut.GetSessionObject();

        // Assert
        Assert.Null(sessionQueryResponse.Value); // defaults
        Assert.False(sessionQueryResponse.HasValue);

        sessionProviderMock.Verify(t => t.GetSession(), Times.Once);
    }

    [Fact]
    public void Session_Contains_Key_Returns_Deserialised_Object()
    {
        // Arrange
        const string stubSessionObjectKey = "key";
        Mock<ISessionObjectKeyResolver> keyResolver = new();
        keyResolver.Setup(t => t.Resolve<StubSessionObject>()).Returns(stubSessionObjectKey);

        byte[] stubTryGetSessionValueBytes = Encoding.UTF8.GetBytes("test-value");
        Mock<ISession> sessionMock = new();
        sessionMock
            .Setup(session => session.TryGetValue(It.IsAny<string>(), out stubTryGetSessionValueBytes!))
            .Returns(true);

        Mock<IAspNetCoreSessionProvider> sessionProviderMock = new();
        sessionProviderMock
            .Setup(provider => provider.GetSession())
            .Returns(sessionMock.Object);

        StubSessionObject stubSessionObject = new();
        Mock<ISessionObjectSerializer<StubSessionObject>> serializer = new();
        serializer
            .Setup((serializer) => serializer.Deserialize(It.IsAny<string>()))
            .Returns(stubSessionObject)
            .Verifiable();

        AspNetCoreSessionQueryHandler<StubSessionObject> sut = new(sessionProviderMock.Object, keyResolver.Object, serializer.Object);

        // Act
        SessionQueryResponse<StubSessionObject> sessionQueryResponse = sut.GetSessionObject();

        // Assert
        Assert.Equal(stubSessionObject, sessionQueryResponse.Value);
        Assert.True(sessionQueryResponse.HasValue);

        sessionProviderMock.Verify(t => t.GetSession(), Times.Once);
        serializer.Verify(t => t.Deserialize("test-value"), Times.Once);
    }

    public class StubSessionObject { }
}
