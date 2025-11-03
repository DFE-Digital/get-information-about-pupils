using System.Text;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock;
public class RequestMatch
{
    public RequestMatch(
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
        char forwardSlash = '/';

        if (string.IsNullOrEmpty(input))
        {
            return forwardSlash.ToString();
        }

        if (input!.StartsWith(forwardSlash.ToString()))
        {
            return input.TrimEnd(forwardSlash);
        }

        return $"/{input}".TrimEnd(forwardSlash);
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

    public static RequestMatch Create(
        string? path,
        HttpMethod method,
        IEnumerable<KeyValuePair<string, string?>> queryParameters)
            => new(path, method, queryParameters);
}
