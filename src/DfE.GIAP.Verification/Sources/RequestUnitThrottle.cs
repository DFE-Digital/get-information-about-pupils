namespace DfE.GIAP.Verification.Sources;

/// <summary>
/// Keeps the request units used in Cosmos DB under a rate, so a comparison doesn't take the throughput the web app
/// needs.
/// </summary>
/// <remarks>
/// It's on top of the Cosmos DB client's own retries of throttled (429) requests, which wait as long as Cosmos DB says
/// to.
/// </remarks>
public sealed class RequestUnitThrottle(double? maxRequestUnitsPerSecond, TimeProvider timeProvider)
{
    private readonly Lock _lock = new();
    private readonly long _start = timeProvider.GetTimestamp();
    private double _requestUnits;

    public async Task RecordAsync(double requestCharge, CancellationToken cancellationToken = default)
    {
        if (maxRequestUnitsPerSecond is not { } max)
        {
            return;
        }

        TimeSpan delay;
        lock (_lock)
        {
            _requestUnits += requestCharge;
            delay = TimeSpan.FromSeconds(_requestUnits / max) - timeProvider.GetElapsedTime(_start);
        }

        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, timeProvider, cancellationToken);
        }
    }
}
