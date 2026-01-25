namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;

public readonly struct PageNumber
{
    public PageNumber(int pageNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);
        Value = pageNumber;
    }

    public int Value { get; }
}
