namespace DfE.GIAP.Core.Search.Common.Application.Adapters;

/// <summary>
/// Represents a standardized set of criteria for configuring search operations.
/// Provides collections of target fields and facet dimensions used to shape search behavior.
/// </summary>
public interface ISearchCriteria
{
    /// <summary>
    /// Gets or sets the list of field names in the source data collection to be searched against.
    /// These are typically indexed fields in the search provider (e.g., Azure Cognitive Search).
    /// </summary>
    IList<string> SearchFields { get; set; }

    /// <summary>
    /// Gets or sets the list of facet identifiers to include in the search request,
    /// allowing for aggregated filtering and refinement in the UI.
    /// </summary>
    IList<string> Facets { get; set; }
}
