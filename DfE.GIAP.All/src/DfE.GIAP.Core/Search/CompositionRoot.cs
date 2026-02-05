using Azure;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions.Factories;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.Mappers;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.Mappers;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.Mappers;
using DfE.GIAP.Core.Search.Infrastructure.Shared;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Builders;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Mappers;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Options;
using DfE.GIAP.Core.Search.Infrastructure.Shared.SearchFilterExpressions;
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
    public static IServiceCollection AddSearchCore(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register core search services and mappers.
        services
            .AddSearchOptions(configuration)
            .AddAzureServices(configuration)
            .AddNationalPupilDatabaseSearchAdaptors()
            .AddPupilPremiumSearchAdaptors()
            .AddFurtherEducationSearchAdaptors()
            .AddFilterExpressions()

            .AddSingleton<
                IMapper<
                    Dictionary<string, IList<AzureFacetResult>>, SearchFacets>,
                    AzureSearchFacetResultsToEstablishmentFacetsMapper>();

        return services;
    }

    private static IServiceCollection AddFurtherEducationSearchAdaptors(this IServiceCollection services)
    {
        services
            .AddScoped<
                IUseCase<
                    FurtherEducationSearchRequest, FurtherEducationSearchResponse>,
                    FurtherEducationSearchUseCase>()
            .AddScoped<
                ISearchServiceAdapter<
                    FurtherEducationLearners, SearchFacets>,
                    AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject>>()
            .AddSingleton<
                IMapper<
                    Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners>,
                    PageableFurtherEducationSearchResultsToLearnerResultsMapper>()
            .AddSingleton<
                IMapper<
                    FurtherEducationLearnerDataTransferObject, FurtherEducationLearner>,
                    FurtherEducationSearchResultToLearnerMapper>();

        return services;
    }

    private static IServiceCollection AddPupilPremiumSearchAdaptors(this IServiceCollection services)
    {
        services
            .AddScoped<
                IUseCase<
                    PupilPremiumSearchRequest, PupilPremiumSearchResponse>,
                    PupilPremiumSearchUseCase>()
            .AddScoped<
                ISearchServiceAdapter<
                    PupilPremiumLearners, SearchFacets>,
                    AzureSearchServiceAdaptor<PupilPremiumLearners, PupilPremiumLearnerDataTransferObject>>()
            .AddSingleton<
                IMapper<
                    Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>>, PupilPremiumLearners>,
                    PageablePupilPremiumSearchResultsToLearnerResultsMapper>()
            .AddSingleton<
                IMapper<
                    PupilPremiumLearnerDataTransferObject, PupilPremiumLearner>,
                    PupilPremiumLearnerDataTransferObjectToPupilPremiumLearnerMapper>();
        return services;
    }

    private static IServiceCollection AddNationalPupilDatabaseSearchAdaptors(this IServiceCollection services)
    {
        services
            .AddScoped<
                IUseCase<
                    NationalPupilDatabaseSearchRequest, NationalPupilDatabaseSearchResponse>,
                    NationalPupilDatabaseSearchUseCase>()
            .AddScoped<
                    ISearchServiceAdapter<NationalPupilDatabaseLearners, SearchFacets>,
                    AzureSearchServiceAdaptor<NationalPupilDatabaseLearners, NationalPupilDatabaseLearnerDataTransferObject>>()
            .AddSingleton<
                IMapper<
                    Pageable<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>>, NationalPupilDatabaseLearners>,
                    PageableNationalPupilDatabaseSearchResultsToNationalPupilDatabaseLearnersMapper>()
            .AddSingleton<
                IMapper<
                    NationalPupilDatabaseLearnerDataTransferObject, NationalPupilDatabaseLearner>,
                    NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper>();
        return services;
    }

    private static IServiceCollection AddSearchOptions(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind Azure Search configuration options.
        services
            .AddOptions<AzureSearchOptions>()
            .Bind(configuration.GetSection(nameof(AzureSearchOptions)));

        // Register strongly typed configuration instances.
        services.AddSingleton(
            serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<AzureSearchOptions>>().Value);

        return services;
    }

    private static IServiceCollection AddAzureServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<ISearchOptionsBuilder, SearchOptionsBuilder>();

        // Register shared cognitive search and filter services.
        services.AddAzureSearchServices(configuration);
        services.AddAzureSearchFilterServices(configuration);

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
