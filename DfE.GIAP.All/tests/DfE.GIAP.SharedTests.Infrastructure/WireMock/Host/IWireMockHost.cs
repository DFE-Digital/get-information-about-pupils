namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Host;
// TODO AsyncDisposable
public interface IWireMockHost : IDisposable
{
    Uri? Endpoint { get; }
    Task StartAsync();
    Task RegisterMappingAsync(MappingRequest mapping);
    Task RegisterMappingsAsync(IEnumerable<MappingRequest> mapping);
}
