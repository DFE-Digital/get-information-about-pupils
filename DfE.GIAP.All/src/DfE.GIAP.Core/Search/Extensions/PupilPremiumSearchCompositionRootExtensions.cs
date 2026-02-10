using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByUniquePupilNumber;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.Mappers;
using DfE.GIAP.Core.Search.Infrastructure.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.Search.Extensions;
internal static class PupilPremiumSearchCompositionRootExtensions
{
    public static IServiceCollection AddPupilPremiumSearchByName(this IServiceCollection services)
    {

        services.TryAddPupilPremiumSearchInfrastructure();

        services
            .AddScoped<
                IUseCase<
                    PupilPremiumSearchByNameRequest, PupilPremiumSearchByNameResponse>,
                    PupilPremiumSearchByNameUseCase>()

            .AddScoped<
                ISearchLearnerByNameService<PupilPremiumLearners>,
                SearchLearnerByNameService<PupilPremiumLearners>>();

        return services;
    }

    public static IServiceCollection AddPupilPremiumSearchByUpn(this IServiceCollection services)
    {
        services.TryAddPupilPremiumSearchInfrastructure();

        services
            .AddScoped<
                IUseCase<
                    PupilPremiumSearchByUniquePupilNumberRequest, PupilPremiumSearchByUniquePupilNumberResponse>,
                    PupilPremiumSearchByUniquePupilNumberUseCase>()

            .AddScoped<
                ISearchLearnersByIdentifierService<PupilPremiumLearners>,
                SearchLearnerByIdentifierService<PupilPremiumLearners>>();

        return services;
    }

    private static IServiceCollection TryAddPupilPremiumSearchInfrastructure(this IServiceCollection services)
    {
        services
            .TryAddScoped<
                ISearchServiceAdapter<PupilPremiumLearners, SearchFacets>,
                AzureSearchServiceAdaptor<PupilPremiumLearners, PupilPremiumLearnerDataTransferObject>>();

        services.TryAddSingleton<
                IMapper<Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>>, PupilPremiumLearners>,
                PageablePupilPremiumSearchResultsToLearnerResultsMapper>();

        services.TryAddSingleton<
                IMapperWithResult<PupilPremiumLearnerDataTransferObject, PupilPremiumLearner>,
                PupilPremiumLearnerDataTransferObjectToPupilPremiumLearnerMapper>();
        return services;
    }
}
