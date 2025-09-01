using DfE.GIAP.Web.Features.Session.Abstractions;
using DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore;
using DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore.Command;
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

    public sealed class StubSessionObject { }
}
