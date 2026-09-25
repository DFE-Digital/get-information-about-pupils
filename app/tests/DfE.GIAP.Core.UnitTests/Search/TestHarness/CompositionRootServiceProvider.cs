using System.Diagnostics.CodeAnalysis;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.UnitTests.Search.TestHarness;

/// <summary>
/// Provides a test harness for composing a fully configured <see cref="IServiceProvider"/>
/// with mock infrastructure services. Enables isolated testing of search orchestration
/// without relying on live Azure Search dependencies.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class CompositionRootServiceProvider
{
    /// <summary>
    /// Sets up a service provider with injected configuration and mocked dependencies.
    /// Replaces infrastructure services with test doubles to support deterministic unit testing.
    /// </summary>
    /// <param name="configuration">The test-specific configuration instance.</param>
    /// <returns>A fully composed <see cref="IServiceProvider"/> for use in test scenarios.</returns>
    public IServiceProvider SetUpServiceProvider(IConfiguration configuration)
    {
        // Initialize a new service collection for dependency registration
        IServiceCollection services = new ServiceCollection();

        // Inject the test configuration as a singleton
        services.AddSingleton(configuration);

        // Register core search dependencies via extension method
        services.AddSearchCore(configuration);

        // Replace ISearchByKeywordService with a mock implementation
        services.RemoveAll<ISearchByKeywordService>();
        ISearchByKeywordService mockSearchService =
            new SearchServiceTestDouble()
                .WithSearchKeywordAndCollection("searchKeyword", "idx-further-education-v3")
                .WithSearchResults(new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>()
                    .WithSearchResults(FurtherEducationLearnerDataTransferObjectTestDouble.Fake())
                    .Create())
                .Create();
        services.AddScoped(provider => mockSearchService);

        // Replace ISearchFilterExpressionsBuilder with a mock implementation
        services.RemoveAll<ISearchFilterExpressionsBuilder>();
        ISearchFilterExpressionsBuilder mockFilterExpressionBuilder =
            FilterExpressionTestDouble.Mock();
        services.AddScoped(provider => mockFilterExpressionBuilder);

        // Finalize and return the composed service provider
        return services.BuildServiceProvider();
    }
}
