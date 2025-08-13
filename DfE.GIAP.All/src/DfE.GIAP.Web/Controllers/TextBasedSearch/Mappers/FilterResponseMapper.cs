using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Core.Search.Common.Application.Models;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;

/// <summary>
/// Maps a single <see cref="SearchFacet"/> to a <see cref="FilterData"/> object.
/// </summary>
public sealed class FilterResponseMapper : IMapper<SearchFacet, FilterData>
{
    /// <summary>
    /// Converts a <see cref="SearchFacet"/> into a <see cref="FilterData"/> structure.
    /// </summary>
    /// <param name="input">The <see cref="SearchFacet"/> containing facet name and result items.</param>
    /// <returns>
    /// A <see cref="FilterData"/> object with the facet name and a list of <see cref="FilterDataItem"/> values.
    /// </returns>
    public FilterData Map(SearchFacet input) =>
        new()
        {
            Name = input.Name,
            Items = input.Results.Select(item => new FilterDataItem()
            {
                Value = item.Value,
                Count = item.Count
            })
            .ToList()
        };
}
