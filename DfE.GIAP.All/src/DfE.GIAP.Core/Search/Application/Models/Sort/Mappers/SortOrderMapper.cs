using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.Search.Shared.Sort.Mappers;

/// <summary>
/// Maps a tuple of (field, direction) into a validated <see cref="SortOrder"/> instance.
/// Used to translate incoming sort parameters into Azure Search-compatible sort expressions.
/// </summary>
public class SortOrderMapper : IMapper<SortOrderRequest, SortOrder>
{
    private readonly SortFieldOptions _sortFieldOptions;

    /// <summary>
    /// Initializes a new instance of <see cref="SortOrderMapper"/>, injecting configured sort field options.
    /// </summary>
    /// <param name="options">
    /// An <see cref="IOptions{SortFieldOptions}"/> instance containing the list of allowed sort fields.
    /// </param>
    public SortOrderMapper(IOptions<SortFieldOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);
        _sortFieldOptions = options.Value;
    }

    /// <summary>
    /// Maps a tuple of (field, direction) into a validated <see cref="SortOrder"/> instance.
    /// If both values are null or empty, defaults to "search.score() desc".
    /// </summary>
    /// <param name="input">Tuple containing the sort field and direction.</param>
    /// <returns>A validated <see cref="SortOrder"/> object.</returns>
    public SortOrder Map(SortOrderRequest input)
    {
        const string DefaultSortField = "search.score()";
        const string DefaultSortDirection = "desc";

        (string? field, string? direction) = input.SortOrder;

        if (!_sortFieldOptions.SortFields.TryGetValue(input.SearchKey, out IReadOnlyList<string>? validSortFields))
        {
            throw new ArgumentException($"Unable to find valid sort fields for the provided search key: {input.SearchKey}.", nameof(input));
        }

        return SortOrder.Create(
            string.IsNullOrEmpty(field) ?
                DefaultSortField :
                    field,
            string.IsNullOrEmpty(direction) ?
                DefaultSortDirection :
                    direction!,
            validSortFields);
    }
}
