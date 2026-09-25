using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

public static class SearchIndexStubServiceCollectionExtensions
{
    /// <summary>
    /// Serves the search index from <paramref name="stub"/> instead of over the network.
    /// <para>
    /// Must be called <em>before</em> <c>AddSearchCore</c>: the Azure Search package registers its
    /// own client provider with <c>TryAddSingleton</c>, which will not displace this one.
    /// </para>
    /// </summary>
    public static IServiceCollection AddStubbedSearchIndex(
        this IServiceCollection services,
        AzureSearchIndexStub stub)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(stub);

        services.AddSingleton(stub);
        services.AddSingleton<ISearchByKeywordClientProvider, StubSearchByKeywordClientProvider>();

        return services;
    }
}
