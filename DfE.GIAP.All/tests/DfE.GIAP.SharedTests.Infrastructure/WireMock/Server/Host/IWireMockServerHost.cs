using DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Client;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Host;
// TODO AsyncDisposable
public interface IWireMockServerHost : IDisposable
{
    Uri Endpoint { get; }
    Task StartAsync();
    IWireMockStubClient CreateClient();
}
