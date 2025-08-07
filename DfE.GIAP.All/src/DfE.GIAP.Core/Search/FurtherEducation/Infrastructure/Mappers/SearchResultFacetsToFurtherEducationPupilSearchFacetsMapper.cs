using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Common.Application.Models;

namespace CognitiveSearch_TestHarness.Search.FurtherEducation.Infrastructure.Mappers;

/// <summary>
/// 
/// </summary>
public sealed class SearchResultFacetsToFurtherEducationPupilSearchFacetsMapper :
    IMapper<Dictionary<string, IList<FacetResult>>, SearchFacets>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public SearchFacets Map(Dictionary<string, IList<FacetResult>> input)
    {
        return new SearchFacets();
    }
}
