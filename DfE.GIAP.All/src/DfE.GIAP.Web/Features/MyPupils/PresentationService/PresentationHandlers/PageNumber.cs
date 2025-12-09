namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

public readonly struct PageNumber
{
    public PageNumber(int page)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        Value = page;
    }

    public int Value { get; }
    public static PageNumber Page(int page) => new(page);
}
