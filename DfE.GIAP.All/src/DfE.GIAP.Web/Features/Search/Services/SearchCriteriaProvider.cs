using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Web.Features.Search.Options;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.Search.Services;

public sealed class SearchCriteriaProvider : ISearchCriteriaProvider
{
    private readonly SearchCriteriaOptions _options;

    public SearchCriteriaProvider(IOptions<SearchCriteriaOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);
        _options = options.Value;
    }

    public SearchCriteria GetCriteria(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_options.Criteria.TryGetValue(key, out SearchCriteria criteria))
        {
            throw new ArgumentException($"Unable to get criteria for key:{key}");
        };

        return criteria!;
    }
}
