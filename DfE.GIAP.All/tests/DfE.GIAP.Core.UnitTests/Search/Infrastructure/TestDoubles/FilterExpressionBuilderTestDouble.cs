using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// A test double for <see cref="ISearchFilterExpressionsBuilder"/> that allows
/// unit tests to control the expected request and the returned filter expression.
/// This enables isolated testing of components that depend on filter expression building
/// without invoking the real implementation.
/// </summary>
internal class FilterExpressionBuilderTestDouble
{
    /// <summary>
    /// The underlying mock instance for <see cref="ISearchFilterExpressionsBuilder"/>.
    /// </summary>
    private readonly Mock<ISearchFilterExpressionsBuilder> _mock = new();

    /// <summary>
    /// The filter expression string that the mock will return when invoked.
    /// </summary>
    private string? _response;

    /// <summary>
    /// The expected <see cref="SearchFilterRequest"/> collection that the mock
    /// will be configured to receive.
    /// </summary>
    private IEnumerable<SearchFilterRequest>? _searchFilterRequests;

    /// <summary>
    /// Configures the test double to return the specified filter expression string
    /// when <see cref="ISearchFilterExpressionsBuilder.BuildSearchFilterExpressions"/> is called.
    /// </summary>
    /// <param name="response">The filter expression string to return.</param>
    /// <returns>The current <see cref="FilterExpressionBuilderTestDouble"/> instance for fluent chaining.</returns>
    public FilterExpressionBuilderTestDouble WithResponse(string response)
    {
        _response = response;
        return this;
    }

    /// <summary>
    /// Configures the test double to expect a specific set of <see cref="SearchFilterRequest"/> objects
    /// when <see cref="ISearchFilterExpressionsBuilder.BuildSearchFilterExpressions"/> is called.
    /// </summary>
    /// <param name="filterRequests">The expected filter requests.</param>
    /// <returns>The current <see cref="FilterExpressionBuilderTestDouble"/> instance for fluent chaining.</returns>
    public FilterExpressionBuilderTestDouble ExpectingRequest(IEnumerable<SearchFilterRequest> filterRequests)
    {
        _searchFilterRequests = filterRequests;
        return this;
    }

    /// <summary>
    /// Creates and returns the configured <see cref="ISearchFilterExpressionsBuilder"/> mock object.
    /// The mock will:
    /// <list type="bullet">
    /// <item>Expect the configured filter requests (if provided).</item>
    /// <item>Return the configured response string (if provided).</item>
    /// <item>Mark the setup as verifiable for later assertion in tests.</item>
    /// </list>
    /// </summary>
    /// <returns>The configured <see cref="ISearchFilterExpressionsBuilder"/> mock object.</returns>
    public ISearchFilterExpressionsBuilder Create()
    {
        // Use the provided response if set; otherwise allow any string.
        string response = _response ?? It.IsAny<string>();

        // Configure the mock to expect the provided filter requests and return the response.
        _mock.Setup(searchFilterExpressionsBuilder =>
            searchFilterExpressionsBuilder.BuildSearchFilterExpressions(_searchFilterRequests!))
            .Returns(response)
            .Verifiable();

        return _mock.Object;
    }
}
