using Azure;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions.Factories;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases;
using DfE.GIAP.Core.Search.Application.UseCases.Request;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.Core.Search.Infrastructure;
using DfE.GIAP.Core.Search.Infrastructure.Builders;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.Mappers;
using DfE.GIAP.Core.Search.Infrastructure.Options;
using DfE.GIAP.Core.Search.Infrastructure.SearchFilterExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    public static IServiceCollection AddSearchDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind Azure Search configuration options.
        services.AddOptions<AzureSearchOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
                configuration
                    .GetSection(nameof(AzureSearchOptions))
                    .Bind(settings));

        // Register core search services and mappers.
        services
            .AddScoped<ISearchServiceAdapter<Learners, SearchFacets>, SearchServiceAdapter>()
            .AddScoped<ISearchOptionsBuilder, SearchOptionsBuilder>()
            .AddSingleton<IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners>, PageableSearchResultsToLearnerResultsMapper>()
            .AddSingleton<IMapper<LearnerDataTransferObject, Learner>, SearchResultToLearnerMapper>()
            .AddSingleton<IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners>, PageableSearchResultsToLearnerResultsMapper>()
            .AddSingleton<IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets>, AzureFacetResultToEstablishmentFacetsMapper>()
            .AddScoped<IUseCase<SearchByKeyWordsRequest, SearchByKeyWordsResponse>, SearchByKeyWordsUseCase>()
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

        // Register shared cognitive search and filter services.
        services.AddDefaultCognitiveSearchServices(configuration);
        services.AddDefaultSearchFilterServices(configuration);

        // Bind search criteria configuration options.
        services.AddOptions<SearchCriteria>()
            .Configure<IConfiguration>((settings, configuration) =>
                configuration
                    .GetSection(nameof(SearchCriteria))
                    .Bind(settings));

        // Bind the SortField configuration options.
        services
            .Configure<SortFieldOptions>(
                configuration.GetSection("SortFields"));

        // Register strongly typed configuration instances.
        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<SearchCriteria>>().Value);

        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<AzureSearchOptions>>().Value);

        return services;
    }
}
