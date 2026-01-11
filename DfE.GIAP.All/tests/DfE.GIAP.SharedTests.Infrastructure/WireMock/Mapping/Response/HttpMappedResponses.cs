namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Response;
public record HttpMappedResponses
{
    private readonly List<HttpMappedResponse> _responses;
    public HttpMappedResponses(IEnumerable<HttpMappedResponse> responses)
    {
        Guard.ThrowIfNullOrEmpty(responses, nameof(responses));
        _responses = responses.ToList();
    }

    public IReadOnlyCollection<HttpMappedResponse> Responses => _responses.AsReadOnly();

    public HttpMappedResponse GetResponseByKey(string key)
    {
        Guard.ThrowIfNullOrWhiteSpace(key, nameof(key));

        HttpMappedResponse? response = _responses.SingleOrDefault(t => t.Key.Equals(key, StringComparison.Ordinal));

        return response is null ?
            throw new ArgumentException($"Unable to find mapped response with identifier: {key}") :
                response;
    }
};
