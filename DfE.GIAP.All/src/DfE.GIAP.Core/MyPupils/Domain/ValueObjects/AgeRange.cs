namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public readonly struct AgeRange
{
    public AgeRange(int low, int high)
    {
        if (low < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(low));
        }
        if (high < 0 || high > 999)
        {
            throw new ArgumentOutOfRangeException(nameof(high));
        }
        Low = low;
        High = high;
        // TODO can AgeRange Low > High? e.g
        // High is not set
        // Or Low is > High which will cause an invalid overlap
    }

    public int High { get; }
    public int Low { get; }
    public bool IsDefaultedRange => IsDefaultLow && IsDefaultHigh;
    public int Range => High - Low;
    private bool IsDefaultLow => Low == 0;
    private bool IsDefaultHigh => High == 0;
}
