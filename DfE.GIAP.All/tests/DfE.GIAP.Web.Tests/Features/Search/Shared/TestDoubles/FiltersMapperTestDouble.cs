using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;
using Moq;

namespace DfE.GIAP.Web.Tests.Features.Search.Shared.TestDoubles;

/// <summary>
/// Provides a reusable test double for mapping SearchFacets to FilterData.
/// Centralizes mock setup for unit tests involving IMapper usage in filter mapping.
/// </summary>
[ExcludeFromCodeCoverage]
internal class FiltersMapperTestDouble
{
    // Static mock instance of the IMapper interface used to simulate mapping behavior.
    // This avoids repeated instantiation and supports consistent setup across tests.
    private static readonly Mock<IMapper<SearchFacets, List<FilterData>>> _mock = new();

    /// <summary>
    /// Returns the shared mock instance for direct configuration or verification.
    /// Useful when tests need to customize behavior or assert mapping calls.
    /// </summary>
    public static Mock<IMapper<SearchFacets, List<FilterData>>> Mock() => _mock;

    /// <summary>
    /// Configures the mock to return a specific list of FilterData
    /// when given a corresponding SearchFacets input.
    /// Enables deterministic mapping behavior for unit tests.
    /// </summary>
    /// <param name="searchFacets">The input search facet model to be mapped.</param>
    /// <param name="filterData">The expected output list of filter data.</param>
    /// <returns>The configured mock instance with mapping setup applied.</returns>
    public static Mock<IMapper<SearchFacets, List<FilterData>>> MockFor(
        SearchFacets searchFacets,
        List<FilterData> filterData)
    {
        // Setup the mock to return the specified filterData when the given searchFacets is mapped.
        _mock.Setup(mapper =>
            mapper.Map(searchFacets)).Returns(filterData);

        // Return the configured mock for use in test assertions or further setup.
        return _mock;
    }
}
