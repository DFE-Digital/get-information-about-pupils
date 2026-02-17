using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.Mappers;
using DfE.GIAP.Core.Search.Infrastructure.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.Search.Extensions;
internal static class NationalPupilDatabaseSearchCompositionRootExtensions
{
    public static IServiceCollection AddNationalPupilDatabaseSearchByName(this IServiceCollection services)
    {
        services.TryAddNationalPupilDatabaseSearchInfrastructure();

        services
            .AddScoped<
                IUseCase<
                    NationalPupilDatabaseSearchByNameRequest, SearchResponse<NationalPupilDatabaseLearners>>,
                    NationalPupilDatabaseSearchByNameUseCase>()

            .AddScoped<
                ISearchLearnerByNameService<NationalPupilDatabaseLearners>,
                SearchLearnerByNameService<NationalPupilDatabaseLearners>>();

        return services;
    }

    public static IServiceCollection AddNationalPupilDatabaseSearchByUpn(this IServiceCollection services)
    {
        services.TryAddNationalPupilDatabaseSearchInfrastructure();

        services
            .AddScoped<
                IUseCase<
                    NationalPupilDatabaseSearchByUniquePupilNumberRequest, SearchResponse<NationalPupilDatabaseLearners>>,
                    NationalPupilDatabaseSearchByUniquePupilNumberUseCase>()

            .AddScoped<
                ISearchLearnersByIdentifierService<NationalPupilDatabaseLearners>,
                SearchLearnerByIdentifierService<NationalPupilDatabaseLearners>>();

        return services;
    }

    private static IServiceCollection TryAddNationalPupilDatabaseSearchInfrastructure(this IServiceCollection services)
    {
        services
            .TryAddScoped<
                ISearchServiceAdapter<NationalPupilDatabaseLearners, SearchFacets>,
                AzureSearchServiceAdaptor<NationalPupilDatabaseLearners, NationalPupilDatabaseLearnerDataTransferObject>>();

        services
            .TryAddSingleton<
                IMapper<
                    Pageable<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>>, NationalPupilDatabaseLearners>,
                    PageableNationalPupilDatabaseSearchResultsToNationalPupilDatabaseLearnersMapper>();

        services.TryAddSingleton<
                IMapperWithResult<
                    NationalPupilDatabaseLearnerDataTransferObject, NationalPupilDatabaseLearner>,
                    NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper>();

        return services;
    }
}
