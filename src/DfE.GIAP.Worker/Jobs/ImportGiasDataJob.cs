using DfE.GIAP.Core.Gias;
using Hangfire;

namespace DfE.GIAP.Worker.Jobs;

/// <remarks>
/// If the job fails, the existing data is left untouched and Hangfire retries it; failures are logged as errors
/// once the retries are exhausted.
/// </remarks>
public class ImportGiasDataJob(GiasImporter importer)
{
    public const string JobId = "import-gias-data";

    // GIAS publishes the day's extracts overnight
    public static string Schedule => Cron.Daily(hour: 6);

    [AutomaticRetry(Attempts = 5)]
    public Task ExecuteAsync(CancellationToken cancellationToken) => importer.ImportAsync(cancellationToken);
}
