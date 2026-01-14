using DfE.GIAP.Web.Shared.Session.Abstraction;
using Moq;

namespace DfE.GIAP.Web.Tests.Shared.Session.TestDoubles;
internal static class ISessionObjectKeyResolverTestDoubles
{
    internal static Mock<ISessionObjectKeyResolver> Default() => new();

    internal static Mock<ISessionObjectKeyResolver> MockFor<TSessionObject>(string resolvedKey)
    {
        Mock<ISessionObjectKeyResolver> keyResolverMock = Default();

        keyResolverMock.Setup(keyResolver => keyResolver.Resolve<TSessionObject>())
            .Returns(resolvedKey)
            .Verifiable();

        return keyResolverMock;
    }

    internal static Mock<ISessionObjectKeyResolver> MockFor<TSessionObject>(Func<string> keyProvider) => MockFor<TSessionObject>(keyProvider());
}
