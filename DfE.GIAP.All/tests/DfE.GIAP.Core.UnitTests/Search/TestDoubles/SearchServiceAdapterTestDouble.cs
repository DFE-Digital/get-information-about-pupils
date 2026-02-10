using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;

namespace DfE.GIAP.Core.UnitTests.Search.TestDoubles;

/// <summary>
/// Provides a test double for <see cref="ISearchServiceAdapter{TResults, TFacets}"/> 
/// used to simulate search adapter behavior in unit tests.
/// Enables deterministic testing of search orchestration logic without invoking real infrastructure.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SearchServiceAdapterTestDouble
{
    public static Mock<ISearchServiceAdapter<TResults, SearchFacets>> MockFor<TResults>(
        ISearchResults<TResults, SearchFacets> searchResults)
    {
        Mock<ISearchServiceAdapter<TResults, SearchFacets>> mock = new();

        mock
            .Setup(adapter => adapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .ReturnsAsync(searchResults)
            .Verifiable();

        return mock;
    }
}
