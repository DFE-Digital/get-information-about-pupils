namespace DfE.GIAP.Core.Search.Application.Models.Sort;

/// <summary>
/// Represents a complete sort configuration for Azure AI Search queries.
/// Combines a validated field and direction into a single sort order expression.
/// </summary>
public sealed class SortOrder
{
    /// <summary>
    /// The validated field to sort by.
    /// </summary>
    private readonly SortField _field;

    /// <summary>
    /// The validated direction of sorting ("asc" or "desc").
    /// </summary>
    private readonly SortDirection _direction;

    /// <summary>
    /// Gets the Azure Search-compatible sort expression (e.g., "Surname desc").
    /// </summary>
    public string Value => $"{_field.Field} {_direction.Direction}";

    /// <summary>
    /// Constructs a new <see cref="SortOrder"/> using validated field and direction.
    /// </summary>
    /// <param name="sortField">The field name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="validSortFields">The list of allowed field names for sorting.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the field or direction is invalid according to their respective validators.
    /// </exception>
    public SortOrder(string sortField, string sortDirection, IReadOnlyList<string> validSortFields)
    {
        _field = SortField.Create(sortField, validSortFields);
        _direction = SortDirection.Create(sortDirection);
    }

    /// <summary>
    /// Static factory method for creating a <see cref="SortOrder"/> instance.
    /// Improves readability and discoverability when constructing validated sort configurations.
    /// </summary>
    /// <param name="field">The field name to sort by.</param>
    /// <param name="direction">The sort direction ("asc" or "desc").</param>
    /// <param name="validFields">The list of allowed field names.</param>
    /// <returns>A validated <see cref="SortOrder"/> instance.</returns>
    public static SortOrder Create(
        string field,
        string direction,
        IReadOnlyList<string> validFields) => new(field, direction, validFields);

    /// <summary>
    /// Returns the sort expression as a string (e.g., "Surname desc").
    /// Useful for diagnostics, logging, or query composition.
    /// </summary>
    public override string ToString() => Value;
}
