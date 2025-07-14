namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction.ValueObjects;
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

    public bool HasLowAge => Low != 0;
    public bool HasHighAge => High != 0;
    public bool HasNoAgeRange => !HasLowAge && !HasHighAge;
    public int Range => High - Low;
    public int High { get; }
    public int Low { get; }
}
