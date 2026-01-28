using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Search;
using AzureFacetResult = Azure.Search.Documents.Models.FacetResult;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// Provides a test double for mapping Azure facet results to domain-specific learner search facets.
/// Used in unit tests to isolate and mock IMapper behavior without invoking real mapping logic.
/// </summary>
internal static class FacetResultToLearnerFacetsMapperTestDouble
{
    /// <summary>
    /// Returns a default mock implementation of IMapper for facet result mapping.
    /// This mock can be injected into tests to simulate mapping behavior without dependencies.
    /// </summary>
    public static Mock<IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets>> DefaultMock() => new();
}
