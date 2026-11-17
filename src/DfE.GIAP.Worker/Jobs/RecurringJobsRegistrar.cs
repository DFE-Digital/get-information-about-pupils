using DfE.GIAP.Core.Jobs;
using Hangfire;

namespace DfE.GIAP.Worker.Jobs;

/// <summary>
/// Adds or updates the recurring job definitions in Hangfire storage when the Worker starts.
/// </summary>
/// <remarks>
/// <see cref="IRecurringJobManager.AddOrUpdate"/> is idempotent, so it's safe for every replica to run this.
/// </remarks>
public sealed class RecurringJobsRegistrar(IRecurringJobManager recurringJobManager) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        recurringJobManager.AddOrUpdate<HeartbeatJob>(
            HeartbeatJob.JobId,
            job => job.ExecuteAsync(CancellationToken.None),
            HeartbeatJob.Schedule);

        recurringJobManager.AddOrUpdate<ImportGiasDataJob>(
            ImportGiasDataJob.JobId,
            job => job.ExecuteAsync(CancellationToken.None),
            ImportGiasDataJob.Schedule);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
