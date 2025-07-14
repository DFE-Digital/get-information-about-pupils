namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;
public readonly struct AgeRange
{
    public AgeRange()
    {
        Low = 0;
        High = 0;
    }

    public AgeRange(int low, int high)
    {
        if(low < 0)
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

    public bool IsDefault => Low == 0 && High == 0;
    public int RangeValue => High - Low;
    public int High { get; }
    public int Low { get; }
}
