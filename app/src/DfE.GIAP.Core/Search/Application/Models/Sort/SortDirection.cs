namespace DfE.GIAP.Core.Search.Application.Models.Sort;

/// <summary>
/// Represents a validated sort direction for Azure AI Search queries.
/// Accepts only "asc" (ascending) or "desc" (descending), and normalizes input to lowercase.
/// </summary>
public sealed class SortDirection
{
    /// <summary>
    /// Constant representing descending sort direction.
    /// </summary>
    private const string Descending = "desc";

    /// <summary>
    /// Constant representing ascending sort direction.
    /// </summary>
    private const string Ascending = "asc";

    /// <summary>
    /// The normalized and validated sort direction string.
    /// Always stored in lowercase ("asc" or "desc") for compatibility with Azure Search.
    /// </summary>
    public string Direction { get; }

    /// <summary>
    /// Constructs a new <see cref="SortDirection"/> after validating and normalizing the input.
    /// </summary>
    /// <param name="direction">
    /// The sort direction string provided by the caller.
    /// Accepts any casing (e.g., "ASC", "Desc") and normalizes to lowercase.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the input is null, empty, or not one of the accepted values ("asc", "desc").
    /// </exception>
    public SortDirection(string direction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(direction);

        // Normalize casing to lowercase using invariant culture for consistency across locales.
        string normalizedSortDirection = direction.ToLowerInvariant();

        // Validate against known sort directions.
        if (!IsValid(normalizedSortDirection))
        {
            throw new ArgumentException($"Unknown sort direction '{normalizedSortDirection}'", nameof(direction));
        }

        // Store the normalized direction
        Direction = normalizedSortDirection;
    }

    /// <summary>
    /// Checks whether the provided direction string is valid.
    /// </summary>
    /// <param name="direction">The normalized direction string to validate.</param>
    /// <returns>True if the direction is "asc" or "desc"; otherwise false.</returns>
    public static bool IsValid(string direction) =>
        direction.Equals(Descending) || direction.Equals(Ascending);

    /// <summary>
    /// Static factory method for creating a <see cref="SortDirection"/> instance.
    /// Improves readability and discoverability when constructing validated sort directions.
    /// </summary>
    /// <param name="direction">The sort direction string to validate and normalize.</param>
    /// <returns>A validated <see cref="SortDirection"/> instance.</returns>
    public static SortDirection Create(string direction) => new(direction);
}
