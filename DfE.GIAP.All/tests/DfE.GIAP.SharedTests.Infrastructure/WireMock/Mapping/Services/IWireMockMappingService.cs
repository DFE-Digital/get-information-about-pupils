using DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Request;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Response;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Services;
internal interface IWireMockMappingService
{
    Task<HttpMappedResponses> RegisterMappingsAsync(IEnumerable<HttpMappingFile> files);
}
