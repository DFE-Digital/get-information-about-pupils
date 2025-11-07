using RestEase;
using WireMock.Admin.Mappings;
using WireMock.Org.RestClient;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Client;
internal sealed class WireMockRefitStubClient : IWireMockStubClient
{
    private readonly IWireMockOrgApi _adminApi;

    public WireMockRefitStubClient(Uri serverAddress)
    {
        _adminApi = RestClient.For<IWireMockOrgApi>(serverAddress);
    }

    public Task Stub<TDataTransferObject>(RequestMatch request, Response<TDataTransferObject> response)
    {
        MappingModelBuilder mappingBuilder = new();

        mappingBuilder
            .WithRequest((r) => r.WithMethods(request.Method).WithUrl(request.PathAndQueryString))
            .WithResponse((res) => res.WithStatusCode(response.StatusCode).WithBodyAsJson(response.Body)));

        return Task.CompletedTask;
        /*_adminApi.PostAdminMappingsAsync();*/
    }
}
