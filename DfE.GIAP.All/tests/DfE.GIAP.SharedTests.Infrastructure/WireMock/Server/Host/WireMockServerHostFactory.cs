using DfE.GIAP.SharedTests.Infrastructure.WireMock.Options;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Host;
internal static class WireMockServerHostFactory
{
    internal static IWireMockServerHost Create(WireMockServerOptions options)
    {
        return options.ServerMode switch
        {
            WireMockServerMode.LocalProcess => new LocalProcessWireMockServerHost(options),
            WireMockServerMode.Remote => new RemoteWireMockServerHost(options),
            _ => throw new NotImplementedException($"Server mode {options.ServerMode} is not implemented."),
        };
    }
}
