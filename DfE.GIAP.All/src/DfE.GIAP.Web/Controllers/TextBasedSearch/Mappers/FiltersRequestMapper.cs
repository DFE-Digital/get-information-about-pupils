using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Common.Application.Models;
using DfE.GIAP.Domain.Search.Learner;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;

/// <summary>
/// Maps a list of <see cref="FilterData"/> objects from the domain layer into a list of 
/// <see cref="FilterRequest"/> objects suitable for the application layer.
/// </summary>
public sealed class FiltersRequestMapper : IMapper<Dictionary<string, string[]>, IList<FilterRequest>>
{
    // Dependency-injected mapper for individual FilterData → FilterRequest conversion
    private readonly IMapper<FilterData, FilterRequest> _filterRequestMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FiltersRequestMapper"/> class.
    /// </summary>
    /// <param name="filterRequestMapper">Mapper used to convert individual filter items.</param>
    /// <exception cref="ArgumentNullException">Thrown if the mapper is null.</exception>
    public FiltersRequestMapper(IMapper<FilterData, FilterRequest> filterRequestMapper)
    {
        _filterRequestMapper = filterRequestMapper ??
            throw new ArgumentNullException(nameof(filterRequestMapper));
    }




    public IList<FilterRequest> Map(Dictionary<string, string[]> input)
    {
        return (input == null) ? [] : input.Select(kvp => new FilterRequest(
            filterName: kvp.Key,
            filterValues: kvp.Value.Cast<object>().ToList())).ToList();
    }
}
