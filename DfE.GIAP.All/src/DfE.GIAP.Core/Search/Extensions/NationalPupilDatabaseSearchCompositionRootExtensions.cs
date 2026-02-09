using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.Mappers;
using DfE.GIAP.Core.Search.Infrastructure.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Search.Extensions;
internal static class NationalPupilDatabaseSearchCompositionRootExtensions
{
    internal static IServiceCollection AddNationalPupilDatabaseSearch(this IServiceCollection services)
    {
        services
            .AddInfrastructure()
            .AddSearchByName()
            .AddSearchByUpn();

        return services;
    }

    private static IServiceCollection AddSearchByName(this IServiceCollection services)
    {
        services
            .AddScoped<
                IUseCase<
                    NationalPupilDatabaseSearchByNameRequest, NationalPupilDatabaseSearchByNameResponse>,
                    NationalPupilDatabaseSearchByNameUseCase>()

            .AddScoped<
                ISearchLearnerByNameService<NationalPupilDatabaseLearners>,
                SearchLearnerByNameService<NationalPupilDatabaseLearners>>();

        return services;
    }

    private static IServiceCollection AddSearchByUpn(this IServiceCollection services)
    {
        services
            .AddScoped<
                IUseCase<
                    NationalPupilDatabaseSearchByUniquePupilNumberRequest, NationalPupilDatabaseSearchByUniquePupilNumberResponse>,
                    NationalPupilDatabaseSearchByUniquePupilNumberUseCase>()

            .AddScoped<
                ISearchLearnersByIdentifierService<NationalPupilDatabaseLearners>,
                SearchLearnerByIdentifierService<NationalPupilDatabaseLearners>>();

        return services;
    }

    private static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<
                ISearchServiceAdapter<NationalPupilDatabaseLearners, SearchFacets>,
                AzureSearchServiceAdaptor<NationalPupilDatabaseLearners, NationalPupilDatabaseLearnerDataTransferObject>>()

            .AddSingleton<
                IMapper<
                    Pageable<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>>, NationalPupilDatabaseLearners>,
                    PageableNationalPupilDatabaseSearchResultsToNationalPupilDatabaseLearnersMapper>()

            .AddSingleton<
                IMapperWithResult<
                    NationalPupilDatabaseLearnerDataTransferObject, NationalPupilDatabaseLearner>,
                    NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper>();

        return services;
    }
}
