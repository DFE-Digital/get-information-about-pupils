namespace DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

/// <summary>
/// A snapshot of a request the application made to the stubbed search index.
/// <para>
/// The body is captured eagerly because <see cref="HttpRequestMessage"/> - and its content stream -
/// is disposed by the SDK once the response has been read.
/// </para>
/// </summary>
/// <param name="Method">The HTTP method used.</param>
/// <param name="RequestUri">The absolute request URI, including the <c>api-version</c> query string.</param>
/// <param name="IndexName">The index the request targeted, or <see langword="null"/> if the path was not a search route.</param>
/// <param name="Body">The serialised request body, empty when the request had no content.</param>
public sealed record RecordedSearchRequest(
    HttpMethod Method,
    Uri RequestUri,
    string? IndexName,
    string Body);
