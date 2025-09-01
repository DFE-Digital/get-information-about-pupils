using DfE.GIAP.Web.Features.Session.Infrastructure.KeyResolver;
using DfE.GIAP.Web.Features.Session.Infrastructure.Provider;
using DfE.GIAP.Web.Features.Session.Query;
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

    private sealed class StubSessionObject { }
}
