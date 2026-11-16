using Hangfire;

namespace DfE.GIAP.Worker.Jobs;

/// <summary>
/// Placeholder recurring job that proves jobs are being scheduled and run in each environment.
/// </summary>
public class HeartbeatJob(ILogger<HeartbeatJob> logger)
{
    public const string JobId = "heartbeat";

    public static string Schedule => Cron.Hourly();

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Heartbeat job ran.");

        return Task.CompletedTask;
    }
}
