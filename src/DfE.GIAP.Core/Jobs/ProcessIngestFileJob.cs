using DfE.GIAP.Core.Ingest;
using Hangfire;

namespace DfE.GIAP.Core.Jobs;

/// <remarks>
/// Files are processed one at a time, in the order they're enqueued, on a queue with a single worker. A failed file
/// isn't retried; it's left in Hangfire's failed jobs, and requeueing the job there replays the file.
/// </remarks>
public class ProcessIngestFileJob(IngestFileProcessor processor)
{
    public const string Queue = "ingest";

    [Queue(Queue)]
    [AutomaticRetry(Attempts = 0)]
    // Stops files being processed at the same time if there's more than one Worker replica
    [DisableConcurrentExecution(timeoutInSeconds: 24 * 60 * 60)]
    public Task ExecuteAsync(string path, CancellationToken cancellationToken) => processor.ProcessAsync(path, cancellationToken);
}
