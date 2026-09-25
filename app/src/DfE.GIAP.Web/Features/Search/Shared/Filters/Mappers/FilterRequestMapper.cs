using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;

namespace DfE.GIAP.Web.Features.Search.Shared.Filters.Mappers;

/// <summary>
/// Maps <see cref="FilterData"/> from the domain layer into a <see cref="FilterRequest"/>.
/// suitable for the application layer.
/// </summary>
public sealed class FilterRequestMapper : IMapper<FilterData, FilterRequest>
{
    /// <summary>
    /// Converts a <see cref="FilterData"/> instance into a <see cref="FilterRequest"/>.
    /// </summary>
    /// <param name="input">The source filter data from the domain.</param>
    /// <returns>A mapped <see cref="FilterRequest"/> with name and values.</returns>
    public FilterRequest Map(FilterData input)
    {
        // Ensure the input is not null to avoid runtime exceptions
        ArgumentNullException.ThrowIfNull(input);

        // Project each item value to object to satisfy IList<object> requirement
        // This cast is necessary because List<string> is not implicitly convertible to List<object>.
        IList<object> filterValues =
            input.Items
                .ConvertAll(item => (object)item.Value);

        // Construct and return the FilterRequest using the mapped values.
        return new FilterRequest(
            filterName: input.Name,
            filterValues: filterValues);
    }
}
