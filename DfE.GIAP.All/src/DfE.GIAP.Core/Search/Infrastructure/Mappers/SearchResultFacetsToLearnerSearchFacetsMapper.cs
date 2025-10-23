using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Search;
using AzureFacetResult = Azure.Search.Documents.Models.FacetResult;

namespace DfE.GIAP.Core.Search.Infrastructure.Mappers;

/// <summary>
/// Maps a dictionary of Azure Cognitive Search facet results into a structured
/// <see cref="SearchFacets"/> model used by the application layer.
/// </summary>
public class AzureFacetResultToEstablishmentFacetsMapper :
    IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets>
{
    /// <summary>
    /// Converts Azure facet results into a domain-friendly <see cref="SearchFacets"/> object.
    /// </summary>
    /// <param name="facetResult">
    /// A dictionary where each key represents a facet category (e.g., "Region", "Type"),
    /// and the value is a list of <see cref="AzureFacetResult"/> entries for that category.
    /// </param>
    /// <returns>
    /// A <see cref="SearchFacets"/> object containing structured facet categories and their values.
    /// </returns>
    public SearchFacets Map(Dictionary<string, IList<AzureFacetResult>> facetResult)
    {
        // Initialize the list to hold mapped facet categories
        List<SearchFacet> searchFacets = [];

        // Iterate over each facet category, skipping null value lists
        foreach (KeyValuePair<string, IList<AzureFacetResult>> facetCategory
            in facetResult.Where(facet => facet.Value != null))
        {
            // Map each Azure facet result into a domain-level FacetResult
            List<FacetResult> values =
                facetCategory.Value.Select(facet =>
                    new FacetResult((string)facet.Value, facet.Count)).ToList();

            // Add the mapped facet category to the result list
            searchFacets.Add(new SearchFacet(facetCategory.Key, values));
        }

        // Return the aggregated facet structure
        return new SearchFacets(searchFacets);
    }
}
