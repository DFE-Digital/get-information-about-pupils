using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;

namespace DfE.GIAP.Web.Features.Search.Shared.Filters.Mappers;

/// <summary>
/// Maps a <see cref="SearchFacets"/> object to a list of <see cref="FilterData"/>.
/// </summary>
public class FiltersResponseMapper : IMapper<SearchFacets, List<FilterData>>
{
    private readonly IMapper<SearchFacet, FilterData> _filterResponseMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FiltersResponseMapper"/> class.
    /// </summary>
    /// <param name="filterResponseMapper">
    /// A mapper that converts individual <see cref="SearchFacet"/> objects to <see cref="FilterData"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="filterResponseMapper"/> is null.
    /// </exception>
    public FiltersResponseMapper(IMapper<SearchFacet, FilterData> filterResponseMapper)
    {
        _filterResponseMapper = filterResponseMapper ??
            throw new ArgumentNullException(nameof(filterResponseMapper));
    }

    /// <summary>
    /// Maps the input <see cref="SearchFacets"/> to a list of <see cref="FilterData"/>.
    /// </summary>
    /// <param name="input">The <see cref="SearchFacets"/> containing facet data to be mapped.</param>
    /// <returns>
    /// A list of <see cref="FilterData"/> objects derived from the input facets.
    /// Returns an empty list if <c>input.Facets</c> is null.
    /// </returns>
    public List<FilterData> Map(SearchFacets input) =>
        input?.Facets == null
            ? [] : input.Facets.ToList().ConvertAll(facet => _filterResponseMapper.Map(facet));
}
