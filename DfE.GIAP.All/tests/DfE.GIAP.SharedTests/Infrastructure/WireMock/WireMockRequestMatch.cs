using System.Text;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock;
public class WireMockRequestMatch
{
    public WireMockRequestMatch(
        string? path,
        HttpMethod method,
        IEnumerable<KeyValuePair<string, string?>>? queryParams)
    {
        Method = method.Method;
        
        Path = NormalisePath(path);

        PathAndQueryString = new StringBuilder()
            .Append(Path)
            .Append(BuildQueryString(queryParams))
            .ToString();
    }

    public string Path { get; }
    public string PathAndQueryString { get; }
    public string Method { get; }

    private static string NormalisePath(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return "/";
        }
        if (input.StartsWith('/'))
        {
            return input.TrimEnd('/');
        }
        return $"/{input}".TrimEnd('/');
    }

    private static string BuildQueryString(IEnumerable<KeyValuePair<string, string?>>? query)
    {
        if (query == null || !query.Any())
        {
            return string.Empty;
        }

        KeyValuePair<string, string?> kv = query.First();
        StringBuilder builder = new($"?{kv.Key}={kv.Value ?? string.Empty}");

        foreach (KeyValuePair<string, string?> item in query.AsEnumerable().Skip(1))
        {
            builder.Append($"&{item.Key}={item.Value ?? string.Empty}");
        }

        return builder.ToString();
    }

    public static WireMockRequestMatch Create(
        string? path,
        HttpMethod method,
        IEnumerable<KeyValuePair<string, string?>> queryParameters)
            => new(path, method, queryParameters);
}
