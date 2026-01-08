using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// Provides test doubles for <see cref="ISearchByKeywordService"/> to support unit testing
/// of search orchestration logic without invoking real Azure Search infrastructure.
/// Captures input parameters for symbolic traceability and diagnostic overlays.
/// </summary>
internal static class SearchByKeywordServiceTestDouble
{
    /// <summary>
    /// Stores the keyword passed to the mocked search service.
    /// Useful for asserting symbolic intent and validating test inputs.
    /// </summary>
    public static string? keywordPassedToSearchService = null;

    /// <summary>
    /// Stores the index name passed to the mocked search service.
    /// Enables traceability of index targeting logic in test scenarios.
    /// </summary>
    public static string? indexPassedToSearchService = null;

    /// <summary>
    /// Stores the search options passed to the mocked search service.
    /// Supports diagnostic overlays and cognitive-effort metrics for query shaping.
    /// </summary>
    public static SearchOptions? searchOptionsPassedToSearchService = null;

    /// <summary>
    /// Returns a default mock of <see cref="ISearchByKeywordService"/> with no configured behavior.
    /// Useful as a baseline for injection into tests that don’t require specific responses.
    /// </summary>
    public static Mock<ISearchByKeywordService> DefaultMock() => new();

    /// <summary>
    /// Returns a mock of <see cref="ISearchByKeywordService"/> configured to return a predefined response.
    /// Captures input parameters for symbolic traceability and enables deterministic test behavior.
    /// </summary>
    /// <param name="searchServiceResponse">The mocked response to return from SearchAsync.</param>
    /// <returns>A configured mock instance for injection into test scenarios.</returns>
    public static Mock<ISearchByKeywordService> MockFor(
        Response<SearchResults<LearnerDataTransferObject>> searchServiceResponse)
    {
        // Create a baseline mock instance
        Mock<ISearchByKeywordService> mockService = DefaultMock();

        // Configure the mock to capture input parameters and return the provided response
        mockService.Setup(service =>
            service.SearchAsync<LearnerDataTransferObject>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOptions>()))
            .Callback<string, string, SearchOptions>((keyword, index, options) =>
            {
                keywordPassedToSearchService = keyword;
                indexPassedToSearchService = index;
                searchOptionsPassedToSearchService = options;
            })
            .Returns(Task.FromResult(searchServiceResponse));

        return mockService;
    }
}
