namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public readonly struct AgeRange
{
    public AgeRange(int low, int high)
    {
        if (low < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(low));
        }
        if (high < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(high));
        }
        Low = low;
        High = high;
    }

    public int High { get; }
    public int Low { get; }
    public bool IsDefaultedRange => IsDefaultLow && IsDefaultHigh;
    public int Range => High - Low;
    private bool IsDefaultLow => Low == 0;
    private bool IsDefaultHigh => High != 0;
}
