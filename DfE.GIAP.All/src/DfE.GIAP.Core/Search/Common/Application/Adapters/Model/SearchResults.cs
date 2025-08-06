namespace DfE.GIAP.Core.Search.Common.Application.Adapters.Model;

/// <summary>
/// Encapsulates the <see cref="EstablishmentResults"/> and <see cref="Facets"/>
/// types that make up the response from the underlying search system.
/// </summary>
public class SearchResults<TResults, TFacetResults>
{
    /// <summary>
    /// The <see cref="TFacetResults"/> returned from the Establishment search
    /// which encapsulates the underlying <see cref="Establishment"/> collection
    /// that is built from the underlying search response.
    /// </summary>
    public TResults? Results { get; init; }

    /// <summary>
    /// The <see cref="TFacetResults"/> returned from the Establishment search
    /// which encapsulates the underlying <see cref="EstablishmentFacet"/> collection
    /// that is built from the underlying search response.
    /// </summary>
    public TFacetResults? FacetResults { get; init; }

    /// <summary>
    /// The Total Count returned from search gives us a total
    /// of all available records which correlates with the given search criteria.
    /// </summary>
    public long? TotalNumberOfRecords { get; init; }
}
