namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Paginate;
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
