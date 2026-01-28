using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Learner.FurtherEducation;
using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.TestDoubles;

/// <summary>
/// Provides a test double for <see cref="ISearchServiceAdapter{TResults, TFacets}"/> 
/// used to simulate search adapter behavior in unit tests.
/// Enables deterministic testing of search orchestration logic without invoking real infrastructure.
/// </summary>
[ExcludeFromCodeCoverage]
public class FurtherEducationSearchServiceAdapterTestDouble
{
    // Shared mock instance for reuse across test scenarios
    private readonly Mock<ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>> _mock = new();

    /// <summary>
    /// Configures the mock adapter to return a predefined <see cref="SearchResults{TResults, TFacets}"/> 
    /// when its SearchAsync method is invoked.
    /// Captures symbolic intent and supports traceable test setups.
    /// </summary>
    /// <param name="searchResults">The mock search results to return.</param>
    /// <returns>A configured mock adapter instance for injection into test scenarios.</returns>
    public Mock<ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>> MockFor(
        SearchResults<FurtherEducationLearners, SearchFacets> searchResults)
    {
        _mock
            .Setup(adapter => adapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .ReturnsAsync(searchResults)
            .Verifiable();

        return _mock;
    }
}
