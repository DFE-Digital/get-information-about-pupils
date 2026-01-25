namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
public sealed class PaginationOptions
{
    public PaginationOptions(int page, int resultsSize)
    {
        Page = new PageNumber(page);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(resultsSize, 0, nameof(resultsSize));
        Size = resultsSize;
    }

    public PageNumber Page { get; }
    public int Size { get; }
}
