﻿using DfE.GIAP.Web.Session.Infrastructure.AspNetCore;
using Microsoft.AspNetCore.Http;
using Moq;

namespace DfE.GIAP.Web.Tests.Session.TestDoubles;
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
