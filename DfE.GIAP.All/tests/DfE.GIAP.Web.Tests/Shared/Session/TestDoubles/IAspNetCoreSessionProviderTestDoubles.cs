using DfE.GIAP.Web.Shared.Session.Infrastructure;
using Microsoft.AspNetCore.Http;
using Moq;

namespace DfE.GIAP.Web.Tests.Shared.Session.TestDoubles;
internal static class IAspNetCoreSessionProviderTestDoubles
{
    internal static Mock<IAspNetCoreSessionProvider> Default() => new();
    internal static Mock<IAspNetCoreSessionProvider> MockWithSession(ISession session)
    {
        Mock<IAspNetCoreSessionProvider> mockProvider = Default();

        mockProvider
            .Setup(provider => provider.GetSession())
            .Returns(session)
            .Verifiable();

        return mockProvider;
    }
}
