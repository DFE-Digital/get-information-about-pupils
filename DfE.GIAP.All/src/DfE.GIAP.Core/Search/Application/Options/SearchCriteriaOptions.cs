using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.Search.Application.Options;
public sealed class SearchCriteriaOptions
{
    public Dictionary<string, SearchCriteria> Criteria { get; set; } = [];
}

public static class SearchCriteriaOptionsExtensions
{
    public static SearchCriteria GetSearchCriteria(this SearchCriteriaOptions options, string key)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!options.Criteria.TryGetValue(key, out SearchCriteria? criteria))
        {
            throw new ArgumentException($"Unable to get criteria for key:{key}");
        };

        return criteria!;
    }
}
