using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DfE.GIAP.Verification.Comparison;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Verification.Sources;

/// <summary>
/// Reads the pupils or learners the old pipeline wrote to one of its Cosmos DB containers.
/// </summary>
public sealed class CosmosRecordSource(CosmosQueries queries, RecordContainer container)
{
    /// <summary>
    /// Every key, in batches. Only the key is read, so this is far cheaper than reading the documents.
    /// </summary>
    public async IAsyncEnumerable<string[]> ReadKeyBatchesAsync(int batchSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // The field names are constants, so they can't be injected
        QueryDefinition query = new($"SELECT VALUE c[\"{container.KeyField}\"] FROM c");

        await foreach (JsonArray page in queries.QueryPagesAsync(query, batchSize, cancellationToken))
        {
            yield return [.. page.Select(k => k?.ToString()).OfType<string>()];
        }
    }

    public async Task<IReadOnlySet<string>> FindExistingAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default)
    {
        QueryDefinition query = new QueryDefinition($"SELECT VALUE c[\"{container.KeyField}\"] FROM c WHERE ARRAY_CONTAINS(@keys, c[\"{container.KeyField}\"])")
            .WithParameter("@keys", keys);

        return (await queries.QueryAsync(query, cancellationToken)).Select(k => k?.ToString()).OfType<string>().ToHashSet(StringComparer.Ordinal);
    }

    public async Task<IReadOnlyDictionary<string, ComparedRecord>> ReadAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default)
    {
        QueryDefinition query = new QueryDefinition($"SELECT * FROM c WHERE ARRAY_CONTAINS(@keys, c[\"{container.KeyField}\"])")
            .WithParameter("@keys", keys);

        Dictionary<string, ComparedRecord> records = [];
        foreach (JsonObject document in (await queries.QueryAsync(query, cancellationToken)).OfType<JsonObject>())
        {
            ComparedRecord record = RecordMappings.FromCosmosDocument(document, container);
            records[record.Key] = record;
        }

        return records;
    }
}
