namespace DfE.GIAP.Core.Search.Application.Models.Sort;

/// <summary>
/// Represents a validated field name used for sorting in Azure AI Search queries.
/// Ensures the field is among a known set of allowed sort fields using case-insensitive matching.
/// </summary>
public sealed class SortField
{
    /// <summary>
    /// The validated field name to sort by.
    /// This value is guaranteed to be one of the allowed fields and retains its original casing.
    /// </summary>
    public string Field { get; }

    /// <summary>
    /// Internal set of allowed field names for sorting.
    /// Uses case-insensitive comparison for validation and is immutable after construction.
    /// </summary>
    private readonly HashSet<string> _validSortFields;

    /// <summary>
    /// Constructs a new <see cref="SortField"/> after validating the input against the allowed field list.
    /// </summary>
    /// <param name="sortField">The field name to sort by (e.g., "Surname", "DOB").</param>
    /// <param name="validSortFields">The list of allowed field names for sorting.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="sortField"/> is null or empty.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="validSortFields"/> is null, empty, or contains duplicates.
    /// Thrown if <paramref name="sortField"/> is not included in the allowed list.
    /// </exception>
    public SortField(string sortField, IReadOnlySet<string> validSortFields)
    {
        ArgumentException.ThrowIfNullOrEmpty(sortField);

        if (validSortFields == null || validSortFields.Count == 0)
        {
            throw new ArgumentException(
                "Valid sort fields list cannot be null or empty.", nameof(validSortFields));
        }

        _validSortFields =
            new HashSet<string>(
                validSortFields, StringComparer.OrdinalIgnoreCase);

        if (!IsValid(sortField))
        {
            throw new ArgumentException($"Unknown sort field '{sortField}'", nameof(sortField));
        }

        Field = sortField;
    }

    /// <summary>
    /// Checks whether the provided field name is valid for sorting.
    /// Comparison is case-insensitive.
    /// </summary>
    /// <param name="field">The field name to validate.</param>
    /// <returns>True if the field is allowed; otherwise false.</returns>
    public bool IsValid(string field) => _validSortFields.Contains(field);

    /// <summary>
    /// Returns a read-only view of the valid sort fields for diagnostics or UI display.
    /// </summary>
    public IReadOnlyCollection<string> ValidFields => _validSortFields;

    /// <summary>
    /// Static factory method for creating a <see cref="SortField"/> instance.
    /// Improves readability and discoverability when constructing validated sort fields.
    /// </summary>
    /// <param name="field">The field name to sort by.</param>
    /// <param name="validFields">The list of allowed field names.</param>
    /// <returns>A validated <see cref="SortField"/> instance.</returns>
    public static SortField Create(
        string field, IReadOnlySet<string> validFields) => new(field, validFields);
}
