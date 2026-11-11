using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// A test double for <see cref="ISearchFilterExpressionsBuilder"/> that allows
/// unit tests to control the expected request and the returned filter expression.
/// This enables isolated testing of components that depend on filter expression building
/// without invoking the real implementation.
/// </summary>
internal static class FilterExpressionTestDouble
{
    private const string DefaultSearchFilterExpressionResponse = "$search.in(category, 'firstname,surname,DOB', ',')";

    /// <summary>
    /// The underlying mock instance for <see cref="ISearchFilterExpressionsBuilder"/>.
    /// </summary>
    private static readonly Mock<ISearchFilterExpressionsBuilder> _mock = new();

    /// <summary>
    /// Returns a mock implementation of <see cref="ISearchFilterExpressionsBuilder"/> 
    /// preconfigured with a default filter expression response.
    /// Used to simulate filter-building logic in unit tests without invoking real expression generation.
    /// </summary>
    public static ISearchFilterExpressionsBuilder Mock() => MockFor(DefaultSearchFilterExpressionResponse);

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
    public static ISearchFilterExpressionsBuilder MockFor(string response)
    {
        // Configure the mock to expect the provided filter requests and return the response.
        _mock.Setup(searchFilterExpressionsBuilder =>
            searchFilterExpressionsBuilder.BuildSearchFilterExpressions(
                It.IsAny<IEnumerable<SearchFilterRequest>>()))
            .Returns(response)
            .Verifiable();

        return _mock.Object;
    }
}
