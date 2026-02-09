using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;

namespace DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

/// <summary>
/// Provides scaffolds for creating <see cref="SearchFacet"/> and <see cref="SearchFacets"/> objects.
/// Supports deterministic construction of facet overlays for filtering and UI diagnostics.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SearchFacetsTestDouble
{
    /// <summary>
    /// Creates a single <see cref="SearchFacet"/> with one result item.
    /// </summary>
    /// <param name="name">Facet name (e.g. "Region").</param>
    /// <param name="value">Facet value (e.g. "North").</param>
    /// <param name="count">Count associated with the facet value.</param>
    /// <returns>A scaffolded <see cref="SearchFacet"/> object.</returns>
    public static SearchFacet CreateFacet(string name, string value, int count) =>
        new(facetName: name, facetResults:
        [
            new FacetResult(value: value, count: count)
        ]);

    /// <summary>
    /// Creates a <see cref="SearchFacets"/> object containing a single facet.
    /// </summary>
    /// <param name="name">Facet name.</param>
    /// <param name="value">Facet value.</param>
    /// <param name="count">Associated count.</param>
    /// <returns>A scaffolded <see cref="SearchFacets"/> object.</returns>
    public static SearchFacets CreateSingleFacetGroup(string name, string value, int count) =>
        new(searchFacets: new List<SearchFacet>
        {
            CreateFacet(name, value, count)
        });

    /// <summary>
    /// Creates a <see cref="SearchFacets"/> object from multiple named facet groups.
    /// </summary>
    /// <param name="facets">A collection of (name, value, count) tuples.</param>
    /// <returns>A scaffolded <see cref="SearchFacets"/> object.</returns>
    public static SearchFacets CreateMultipleFacetGroups(
        IEnumerable<(string Name, string Value, int Count)> facets)
    {
        List<SearchFacet> facetList = [];
        foreach ((string name, string value, int count) in facets)
        {
            facetList.Add(CreateFacet(name, value, count));
        }

        return new(searchFacets: facetList);
    }

    public static SearchFacets Stub()
    {
        List<SearchFacet> facets =
        [
            CreateFacet(name: "name", value: "value1", count: 1)
        ];

        return new SearchFacets(facets);
    }
}
