using Azure;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Common.Application.Adapters;
using DfE.GIAP.Core.Search.Common.Application.Models;
using DfE.GIAP.Core.Search.Common.Infrastructure.Builders;
using DfE.GIAP.Core.Search.Common.Infrastructure.Options;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Request;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Response;
using DfE.GIAP.Core.Search.FurtherEducation.Infrastructure;
using DfE.GIAP.Core.Search.FurtherEducation.Infrastructure.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Dto = DfE.GIAP.Core.Search.FurtherEducation.Infrastructure.DataTransferObjects;
using AzureFacetResult = Azure.Search.Documents.Models.FacetResult;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions.Factories;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions;
using DfE.GIAP.Core.Search.FurtherEducation.Infrastructure.SearchFilterExpressions;

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

        return services.RegisterFurtherEducationSearchDependencies(configuration);
    }

    /// <summary>
    /// Registers Further Education-specific search services, mappers, options, and use cases.
    /// </summary>
    /// <param name="services">The service collection to register dependencies into.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection RegisterFurtherEducationSearchDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind Azure Search configuration options
        services.AddOptions<AzureSearchOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
                configuration
                    .GetSection(nameof(AzureSearchOptions))
                    .Bind(settings));

        // Register core search services and mappers
        services
            .AddScoped<ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>, FurtherEducationSearchServiceAdapter>()
            .AddScoped<ISearchOptionsBuilder, SearchOptionsBuilder>()
            .AddSingleton<IMapper<Pageable<SearchResult<Dto.FurtherEducationLearner>>, FurtherEducationLearners>, PageableSearchResultsToFurtherEducationLearnerResultsMapper>()
            .AddSingleton<IMapper<Dto.FurtherEducationLearner, FurtherEducationLearner>, SearchResultToFurtherEducationLearnerMapper>()
            .AddSingleton<IMapper<Pageable<SearchResult<Dto.FurtherEducationLearner>>, FurtherEducationLearners>, PageableSearchResultsToFurtherEducationLearnerResultsMapper>()
            .AddSingleton<IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets>, AzureFacetResultToEstablishmentFacetsMapper>()
            .AddScoped<IUseCase<SearchByFirstNameAndOrSurnameRequest, SearchByFirstNameAndOrSurnameResponse>, SearchByFirstNameAndOrSurnameUseCase>()
            .AddScoped<SearchCollectionValuedFilterExpression>();

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
                            ServiceProvider.GetRequiredService<SearchCollectionValuedFilterExpression>()
                };

            return new SearchFilterExpressionFactory(searchFilterExpressions);
        });


        // Register shared cognitive search and filter services
        services.AddDefaultCognitiveSearchServices(configuration);
        services.AddDefaultSearchFilterServices(configuration);

        // Bind search criteria configuration options
        services.AddOptions<SearchCriteria>()
            .Configure<IConfiguration>((settings, configuration) =>
                configuration
                    .GetSection(nameof(SearchCriteria))
                    .Bind(settings));

        // Register strongly typed configuration instances
        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<SearchCriteria>>().Value);

        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<AzureSearchOptions>>().Value);

        return services;
    }
}
