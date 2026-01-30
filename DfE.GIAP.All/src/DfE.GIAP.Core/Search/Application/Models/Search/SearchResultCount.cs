namespace DfE.GIAP.Core.Search.Application.Models.Search;
public readonly struct SearchResultCount
{
    public SearchResultCount(int? resultCount = null)
    {
        Count = resultCount is null || resultCount < 0 ?
            0 :
                resultCount.Value;
    }

    public int Count { get; }

    public static implicit operator int(SearchResultCount count) => count.Count;
}
