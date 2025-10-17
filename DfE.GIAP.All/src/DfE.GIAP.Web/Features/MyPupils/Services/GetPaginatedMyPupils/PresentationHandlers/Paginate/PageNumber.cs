namespace DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils.PresentationHandlers.Paginate;

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
