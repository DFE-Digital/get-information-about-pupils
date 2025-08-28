using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Domain.Search.Learner;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;

/// <summary>
/// Maps a dictionary of raw filter name/value pairs (from the web layer)
/// into a list of structured <see cref="FilterRequest"/> objects suitable
/// for the application layer. Delegates individual filter mapping to an injected
/// <see cref="IMapper{FilterData, FilterRequest}"/> for consistency and reuse.
/// </summary>
public sealed class FiltersRequestMapper : IMapper<Dictionary<string, string[]>, IList<FilterRequest>>
{
    // Dependency-injected mapper for converting individual FilterData → FilterRequest.
    // Promotes modularity, testability, and reuse across filter mapping work-flows.
    private readonly IMapper<FilterData, FilterRequest> _filterRequestMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FiltersRequestMapper"/> class.
    /// </summary>
    /// <param name="filterRequestMapper">
    /// Mapper used to convert individual filter entries from domain to application format.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the injected mapper is null, ensuring safe construction.
    /// </exception>
    public FiltersRequestMapper(IMapper<FilterData, FilterRequest> filterRequestMapper)
    {
        _filterRequestMapper = filterRequestMapper ??
            throw new ArgumentNullException(nameof(filterRequestMapper));
    }

    /// <summary>
    /// Maps a dictionary of filter name → string[] values into a list of <see cref="FilterRequest"/> objects.
    /// Each dictionary entry is transformed into a <see cref="FilterData"/> and passed to the injected mapper.
    /// </summary>
    /// <param name="input">
    /// Dictionary of raw filter values, typically from the web request or controller layer.
    /// </param>
    /// <returns>
    /// A list of <see cref="FilterRequest"/> objects suitable for downstream application logic.
    /// </returns>
    public IList<FilterRequest> Map(Dictionary<string, string[]> input)
    {
        // Return an empty list if the input is null or contains no filters.
        // This avoids null propagation and simplifies downstream handling.
        if (input == null || input.Count == 0)
            return [];

        // Transform each dictionary entry into a FilterData object,
        // then delegate mapping to the injected FilterRequestMapper.
        return input.Select(kvp =>
        {
            // Construct domain-layer FilterData from raw key/value pair.
            FilterData filterData = new()
            {
                Name = kvp.Key,
                Items = kvp.Value.Select(value =>
                    new FilterDataItem { Value = value }).ToList()
            };

            // Delegate conversion to the injected mapper.
            return _filterRequestMapper.Map(filterData);
        })
        .ToList();
    }
}
