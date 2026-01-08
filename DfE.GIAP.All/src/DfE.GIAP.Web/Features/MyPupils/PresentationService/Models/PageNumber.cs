namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;

public readonly struct PageNumber
{
    public PageNumber(int pageNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);
        Value = pageNumber;
    }

    public int Value { get; }
}
