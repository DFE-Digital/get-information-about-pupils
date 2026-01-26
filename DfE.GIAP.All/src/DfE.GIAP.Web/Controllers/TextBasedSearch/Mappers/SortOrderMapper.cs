using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;

/// <summary>
/// Maps a tuple of (field, direction) into a validated <see cref="SortOrder"/> instance.
/// Used to translate incoming sort parameters into Azure Search-compatible sort expressions.
/// </summary>
public class SortOrderMapper : IMapper<(string Field, string Direction), SortOrder>
{
    private readonly IReadOnlyList<string> _sortFieldOptions;

    /// <summary>
    /// Initializes a new instance of <see cref="SortOrderMapper"/>, injecting configured sort field options.
    /// </summary>
    /// <param name="options">
    /// An <see cref="IOptions{SortFieldOptions}"/> instance containing the list of allowed sort fields.
    /// </param>
    public SortOrderMapper(IOptions<SortFieldOptions> options)
    {
        _sortFieldOptions = options?.Value?.ValidFields
            ?? throw new ArgumentNullException(nameof(options), "Sort field options must be provided.");
    }

    /// <summary>
    /// Maps a tuple of (field, direction) into a validated <see cref="SortOrder"/> instance.
    /// If both values are null or empty, defaults to "search.score() desc".
    /// </summary>
    /// <param name="input">Tuple containing the sort field and direction.</param>
    /// <returns>A validated <see cref="SortOrder"/> object.</returns>
    public SortOrder Map((string Field, string Direction) input)
    {
        const string DefaultSortField = "search.score()";
        const string DefaultSortDirection = "desc";

        (string field, string direction) = input;

        if (string.IsNullOrEmpty(field) && string.IsNullOrEmpty(direction))
        {
            field = DefaultSortField;
            direction = DefaultSortDirection;
        }

        return SortOrder.Create(field, direction, _sortFieldOptions);
    }
}
