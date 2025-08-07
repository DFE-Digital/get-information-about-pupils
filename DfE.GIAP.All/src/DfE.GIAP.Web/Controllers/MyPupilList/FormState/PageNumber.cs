namespace DfE.GIAP.Web.Controllers.MyPupilList.FormState;
public readonly struct PageNumber
{
    public PageNumber(int page)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        Value = page;
    }

    public int Value { get; }

    public static PageNumber Default => Page(1);
    public static PageNumber Page(int page) => new(page);
}
