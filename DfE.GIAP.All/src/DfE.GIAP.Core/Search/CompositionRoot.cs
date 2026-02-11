using Dfe.Data.Common.Infrastructure.CognitiveSearch;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions.Factories;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Options.Search;
using DfE.GIAP.Core.Search.Extensions;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Builders;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Mappers;
using DfE.GIAP.Core.Search.Infrastructure.Shared.SearchFilterExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AzureFacetResult = Azure.Search.Documents.Models.FacetResult;

namespace DfE.GIAP.Core.Search;

/// <summary>
/// Provides extension methods to register search-related services and dependencies
/// for the Further Education domain into the application's dependency injection container.
/// </summary>
public static class CompositionRoot
{
    /// <summary>
    /// Registers all search dependencies required for Further Education search functionality.
    /// </summary>
    /// <param name="services">The service collection to register dependencies into.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSearchCore(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddSearchOptions(configuration);

        services
            .AddTransient<ISearchOptionsBuilder, SearchOptionsBuilder>();

        services.AddSingleton<IMapper<SearchCriteriaOptions, SearchCriteria>, SearchCriteriaOptionsToSearchCriteriaMapper>();
        // Register shared cognitive search and filter services.
        services.AddAzureSearchServices(configuration);
        services.AddAzureSearchFilterServices(configuration);

        services
            .AddNationalPupilDatabaseSearchByName()
            .AddNationalPupilDatabaseSearchByUpn()
            .AddPupilPremiumSearchByName()
            .AddPupilPremiumSearchByUpn()
            .AddFurtherEducationSearchByName()
            .AddFurtherEducationSearchByUniqueLearnerNumber()
            .AddFilterExpressions()
            .AddSingleton<
                IMapper<
                    Dictionary<string, IList<AzureFacetResult>>, SearchFacets>,
                    AzureSearchFacetResultsToEstablishmentFacetsMapper>();

        return services;
    }

    private static IServiceCollection AddFilterExpressions(this IServiceCollection services)
    {
        services
            .AddScoped<SearchCollectionValuedFilterExpression>()
            .AddScoped<SearchByEqualityFilterExpression>();

        services.AddSingleton<ISearchFilterExpressionFactory>(provider =>
        {
            IServiceScope scopedSearchFilterExpressionProvider = provider.CreateScope();
            Dictionary<string, Func<ISearchFilterExpression>> searchFilterExpressions =
                new()
                {
                    ["SearchInFilterExpression"] = () =>
                        scopedSearchFilterExpressionProvider
                            .ServiceProvider.GetRequiredService<SearchInFilterExpression>(),
                    ["LessThanOrEqualToExpression"] = () =>
                        scopedSearchFilterExpressionProvider
                            .ServiceProvider.GetRequiredService<LessThanOrEqualToExpression>(),
                    ["SearchCollectionValuedFilterExpression"] = () =>
                        scopedSearchFilterExpressionProvider.
                            ServiceProvider.GetRequiredService<SearchCollectionValuedFilterExpression>(),
                    ["SearchByEqualityFilterExpression"] = () =>
                        scopedSearchFilterExpressionProvider
                            .ServiceProvider.GetRequiredService<SearchByEqualityFilterExpression>()
                };

            return new SearchFilterExpressionFactory(searchFilterExpressions);
        });
        return services;
    }
}
