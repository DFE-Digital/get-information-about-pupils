using DfE.GIAP.SharedTests.Infrastructure.WireMock.Client.Java.DataTransferObjects;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Client.Java;
public sealed class WireMockJavaRemoteClient : IWireMockRemoteClient
{
    private readonly HttpClient _client;
    private readonly RemoteWireMockHostServerOptions _serverOptions;
    private readonly IMapper<MappingModel, WireMockJavaMappingRequestDto> _mappingModelToWireMockJavaMappingRequestMapper;

    public WireMockJavaRemoteClient(
        HttpClient client,
        RemoteWireMockHostServerOptions serverOptions,
        IMapper<MappingModel, WireMockJavaMappingRequestDto> mappingModelToWireMockJavaMappingRequestMapper)
    {
        Guard.ThrowIfNull(client, nameof(client));
        _client = client;

        Guard.ThrowIfNull(serverOptions, nameof(serverOptions));
        _serverOptions = serverOptions;

        Guard.ThrowIfNull(mappingModelToWireMockJavaMappingRequestMapper, nameof(mappingModelToWireMockJavaMappingRequestMapper));
        _mappingModelToWireMockJavaMappingRequestMapper = mappingModelToWireMockJavaMappingRequestMapper;
    }

    public Task PostMappingsAsync(MappingModel mapping)
    {
        Guard.ThrowIfNull(mapping, nameof(mapping));

        WireMockJavaMappingRequestDto request = _mappingModelToWireMockJavaMappingRequestMapper.Map(mapping);

        StringContent content = new(
                content: JsonConvert.SerializeObject(request),
                encoding: Encoding.UTF8,
                mediaType: "application/json");

        return _client.PostAsync(_serverOptions.MappingEndpoint, content);
    }
}


public interface IMapper<in Tin, out TOut>
{
    TOut Map(Tin input);
}
