using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Web.Features.Search.Options.Sort;

namespace DfE.GIAP.Web.Features.Search.Shared.Sort;

internal sealed class SortOrderFactory : ISortOrderFactory
{
    public SortOrder Create(SortOptions options, (string? field, string? direction) sort)
    {
        ArgumentNullException.ThrowIfNull(options);

        (string defaultField, string defaultDirection) = options.GetDefaultSort();

        List<string> validSortFields = [];
        validSortFields.Add(defaultDirection);
        validSortFields.AddRange(options.Fields);

        return SortOrder.Create(
            field: string.IsNullOrWhiteSpace(sort.field) ? defaultField : sort.field,
            direction: string.IsNullOrWhiteSpace(sort.direction) ? defaultDirection : sort.direction,
            validFields: validSortFields);
    }
}
