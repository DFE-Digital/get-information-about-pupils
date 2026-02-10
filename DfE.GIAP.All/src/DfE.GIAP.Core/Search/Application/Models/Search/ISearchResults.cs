namespace DfE.GIAP.Core.Search.Application.Models.Search;

/// <summary>
/// Encapsulates the <see cref="EstablishmentResults"/> and <see cref="Facets"/>
/// types that make up the response from the underlying search system.
/// </summary>
public interface ISearchResults<TResults, TFacetResults>
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
}

public record SearchResults<TResults, TFacetResults> : ISearchResults<TResults, TFacetResults>
{
    public TResults? Results { get; init; }
    public TFacetResults? FacetResults { get; init; }
}
