namespace DfE.GIAP.Web.Features.Search.Options;

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
    public Dictionary<string, IReadOnlyList<string>> SortFields { get; init; } = [];
}
