using System.Net;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Response;
public record HttpMappedResponse
{
    private readonly string _body;

    public HttpMappedResponse(
        MappingKey key,
        int statusCode,
        string? body = null)
    {
        Guard.ThrowIfNull(key, nameof(key));
        Key = key.Value;
        StatusCode = statusCode;
        _body = body ?? string.Empty;
    }

    public HttpMappedResponse(
        MappingKey mappingId,
        HttpStatusCode code,
        string body) : this(mappingId, (int)code, body)
    { }

    public string Key { get; }
    public int StatusCode { get; }

    public TBody GetResponseBody<TBody>()
    {
        if (string.IsNullOrWhiteSpace(_body))
        {
            throw new ArgumentException("Response body is empty.");
        }

        return JsonConvert.DeserializeObject<TBody>(_body)
            ?? throw new ArgumentException($"Failed to deserialize body to {typeof(TBody).Name}");
    }

    public static HttpMappedResponse Create(MappingKey clientId, int code, string body) => new(clientId, code, body);
}
