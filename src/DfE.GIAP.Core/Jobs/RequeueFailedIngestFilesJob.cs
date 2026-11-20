using DfE.GIAP.Core.Ingest;
using Hangfire;

namespace DfE.GIAP.Core.Jobs;

/// <summary>
/// Replays every ingest file whose latest processing failed. It's never scheduled; trigger it from the Hangfire
/// dashboard.
/// </summary>
public class RequeueFailedIngestFilesJob(IngestScheduler scheduler)
{
    public const string JobId = "requeue-failed-ingest-files";

    public static string Schedule => Cron.Never();

    [AutomaticRetry(Attempts = 0)]
    public Task ExecuteAsync(CancellationToken cancellationToken) => scheduler.RequeueFailedAsync(cancellationToken);
}
