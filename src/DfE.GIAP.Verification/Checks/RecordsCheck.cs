using DfE.GIAP.Verification.Comparison;
using DfE.GIAP.Verification.Reports;
using DfE.GIAP.Verification.Sources;

namespace DfE.GIAP.Verification.Checks;

/// <summary>
/// Compares the pupils or learners in Postgres with the documents in one of the old pipeline's containers.
/// </summary>
public sealed class RecordsCheck(
    RecordContainer container,
    PostgresRecordSource newSource,
    CosmosRecordSource oldSource,
    CheckOptions options)
{
    /// <summary>
    /// Compares the given pupils or learners.
    /// </summary>
    public Task CompareKeysAsync(IReadOnlyCollection<string> keys, VerificationReport report, CancellationToken cancellationToken = default) =>
        Parallel.ForEachAsync(
            keys.Order(StringComparer.Ordinal).Chunk(options.BatchSize),
            new ParallelOptions { MaxDegreeOfParallelism = options.Concurrency, CancellationToken = cancellationToken },
            (batch, ct) => new ValueTask(CompareBatchAsync(batch, report, ct)));

    /// <summary>
    /// Compares every pupil or learner on either side.
    /// </summary>
    /// <remarks>
    /// Every pupil in Postgres is compared with its document, then every document's key is checked against Postgres to
    /// find the documents with no pupil. That way neither side's keys need to be held in memory.
    /// </remarks>
    public async Task CompareAllAsync(VerificationReport report, CancellationToken cancellationToken = default)
    {
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = options.Concurrency, CancellationToken = cancellationToken };

        await Parallel.ForEachAsync(
            newSource.ReadKeyBatchesAsync(options.BatchSize, cancellationToken),
            parallelOptions,
            (batch, ct) => new ValueTask(CompareBatchAsync(batch, report, ct)));

        await Parallel.ForEachAsync(
            oldSource.ReadKeyBatchesAsync(options.BatchSize, cancellationToken),
            parallelOptions,
            async (batch, ct) =>
            {
                IReadOnlySet<string> existing = await newSource.FindExistingAsync(batch, ct);
                string[] missing = [.. batch.Where(k => !existing.Contains(k))];

                foreach (string key in missing)
                {
                    report.Add(new Difference(container.Name, DifferenceKinds.MissingInNew, Dataset: null, Field: null, key));
                }

                report.AddCompared(container.Name, missing.Length);
            });
    }

    private async Task CompareBatchAsync(string[] keys, VerificationReport report, CancellationToken cancellationToken)
    {
        Task<IReadOnlyDictionary<string, ComparedRecord>> newRecords = newSource.ReadAsync(keys, cancellationToken);
        Task<IReadOnlyDictionary<string, ComparedRecord>> oldRecords = oldSource.ReadAsync(keys, cancellationToken);
        await Task.WhenAll(newRecords, oldRecords);

        foreach (string key in keys)
        {
            foreach (Difference difference in RecordComparer.Compare(
                container.Name,
                key,
                (await newRecords).GetValueOrDefault(key),
                (await oldRecords).GetValueOrDefault(key)))
            {
                report.Add(difference);
            }
        }

        report.AddCompared(container.Name, keys.Length);
    }
}

/// <param name="BatchSize">How many records are read from each side at a time.</param>
/// <param name="Concurrency">How many batches are compared at once.</param>
public sealed record CheckOptions(int BatchSize, int Concurrency);
