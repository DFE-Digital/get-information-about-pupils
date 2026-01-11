using DfE.GIAP.SharedTests.Infrastructure.WireMock.Host;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Request;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Response;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Services;
internal sealed class WireMockMappingService : IWireMockMappingService
{
    private readonly IWireMockHost _server;
    private readonly MappingResolver _mappingResolver;

    public WireMockMappingService(IWireMockHost server)
    {
        Guard.ThrowIfNull(server, nameof(server));
        _server = server;

        _mappingResolver = new MappingResolver(rootDirectory: "contracts"); // TODO make configurable through Options when SearchLocations : [] when Stub discovery tackled
    }

    public async Task<HttpMappedResponses> RegisterMappingsAsync(IEnumerable<HttpMappingFile> files)
    {
        List<MappingRequest> mappingRequests =
            files.Select((file) =>
                new MappingRequest(
                    id: new MappingKey(file.Key),
                    model: _mappingResolver.ResolveMapping(file.FileName))).ToList();

        await _server.RegisterMappingsAsync(mappingRequests);

        return new HttpMappedResponses(
            responses: mappingRequests.Select(MapMappingRequestToHttpMappedResponse));
    }

    private static HttpMappedResponse MapMappingRequestToHttpMappedResponse(MappingRequest model)
    {
        Guard.ThrowIfNull(model, nameof(model));

        return HttpMappedResponse.Create(
            model.ClientId,
            code: Convert.ToInt32(model.Mapping.Response.StatusCode),
            body: model.Mapping.Response.BodyAsJson!.ToString());
    }
}
