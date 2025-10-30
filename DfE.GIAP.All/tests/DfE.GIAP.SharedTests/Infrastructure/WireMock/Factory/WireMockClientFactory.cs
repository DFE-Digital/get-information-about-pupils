using DfE.GIAP.SharedTests.Infrastructure.WireMock.Client;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Options;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Factory;
internal static class WireMockClientFactory
{
    internal static IWireMockClient Create(WireMockServerOptions options)
    {
        return options.ServerMode switch
        {
            WireMockServerMode.InProcess => new WireMockLocalClient(options),
            WireMockServerMode.Remote => new WireMockRemoteClient(options),
            _ => throw new NotImplementedException($"Server mode {options.ServerMode} is not implemented."),
        };
    }
}
