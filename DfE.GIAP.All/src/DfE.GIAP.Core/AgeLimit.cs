namespace DfE.GIAP.Core;
public readonly struct AgeLimit
{
    public AgeLimit(int low, int high)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(low);
        ArgumentOutOfRangeException.ThrowIfNegative(high);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(high, 999);
        Low = low;
        High = high;
    }

    public int High { get; }
    public int Low { get; }
    public bool IsDefaultLimit => IsLowDefaultedOrNotSet && IsHighDefaultedOrNotSet;
    private bool IsLowDefaultedOrNotSet => Low == 0;
    private bool IsHighDefaultedOrNotSet => High == 0;
}
