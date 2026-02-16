using DfE.GIAP.SharedTests.Infrastructure.WireMock.Client.Java.DataTransferObjects;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Client.Java;
internal sealed class MappingModelToWireMockJavaMappingRequestMapper
{
    public WireMockJavaMappingRequestDto Map(MappingModel input)
    {
        Guard.ThrowIfNull(input, nameof(input));
        PathModel? requestPathMatcher = (input.Request.Path as JObject)?.ToObject<PathModel>(); // TODO better inference for different paths
        Guard.ThrowIfNull(requestPathMatcher, nameof(requestPathMatcher));

        string url = requestPathMatcher!.Matchers.First().Pattern!.ToString();

        return new()
        {
            request = new WireMockJavaMappingRequestRequestModel()
            {
                urlPath = url
            },
            response = new WireMockJavaMappingRequestResponseModel()
            {
                status = int.Parse(input.Response.StatusCode?.ToString()),
                headers = input.Response.Headers ?? new Dictionary<string, object>(),
                jsonBody = input.Response.BodyAsJson
            }
        };
    }
}

