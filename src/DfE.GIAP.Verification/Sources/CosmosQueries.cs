using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Verification.Sources;

/// <summary>
/// Runs queries against one of the old pipeline's containers, reading the documents as raw JSON so that nothing is
/// lost or converted by a serializer.
/// </summary>
public sealed class CosmosQueries(Container container, RequestUnitThrottle throttle)
{
    /// <summary>
    /// The results of a query, a page at a time.
    /// </summary>
    public async IAsyncEnumerable<JsonArray> QueryPagesAsync(
        QueryDefinition query,
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using FeedIterator iterator = container.GetItemQueryStreamIterator(
            query,
            requestOptions: new QueryRequestOptions { MaxItemCount = pageSize });

        while (iterator.HasMoreResults)
        {
            using ResponseMessage response = await iterator.ReadNextAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            await throttle.RecordAsync(response.Headers.RequestCharge, cancellationToken);

            JsonNode? page = await JsonNode.ParseAsync(response.Content, cancellationToken: cancellationToken);
            if (page?["Documents"] is JsonArray documents && documents.Count > 0)
            {
                yield return documents;
            }
        }
    }

    public async Task<List<JsonNode?>> QueryAsync(QueryDefinition query, CancellationToken cancellationToken = default)
    {
        List<JsonNode?> results = [];

        await foreach (JsonArray page in QueryPagesAsync(query, pageSize: 1000, cancellationToken))
        {
            results.AddRange(page.Select(n => n?.DeepClone()));
        }

        return results;
    }
}
