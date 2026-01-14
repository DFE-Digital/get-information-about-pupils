namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Client;
public interface IWireMockRemoteClient
{
    Task PostMappingsAsync(MappingModel mapping);
}
