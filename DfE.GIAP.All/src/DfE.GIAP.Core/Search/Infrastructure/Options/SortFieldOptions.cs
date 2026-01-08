namespace DfE.GIAP.Core.Search.Application.Models.Search;

/// <summary>
/// Represents a configurable set of valid field names that can be used for sorting
/// in Azure AI Search queries. Typically injected via <see cref="Microsoft.Extensions.Options.IOptions{T}"/>.
/// </summary>
public sealed class SortFieldOptions
{
    /// <summary>
    /// Gets or sets the list of allowed field names for sorting.
    /// These should match the field names defined in the Azure Search index schema.
    /// </summary>
    public IReadOnlyList<string> ValidFields { get; init; } = [];
}
