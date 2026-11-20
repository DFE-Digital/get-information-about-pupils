using DfE.GIAP.Core.Ingest;
using Hangfire;

namespace DfE.GIAP.Core.Jobs;

public class PollIngestUploadsJob(IngestScheduler scheduler)
{
    public const string JobId = "poll-ingest-uploads";

    public static string Schedule => Cron.Minutely();

    // The next poll is a minute away, so there's no point retrying
    [AutomaticRetry(Attempts = 0, LogEvents = false)]
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public Task ExecuteAsync(CancellationToken cancellationToken) => scheduler.ScanUploadsAsync(cancellationToken);
}
