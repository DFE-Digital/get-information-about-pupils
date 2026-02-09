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

namespace DfE.GIAP.Core.Search.Extensions;
internal static class PupilPremiumSearchCompositionRootExtensions
{
    internal static IServiceCollection AddPupilPremiumSearch(this IServiceCollection services)
    {
        services.AddSearchByName()
            .AddSearchByUpn()
            .AddInfrastructure();

        return services;
    }

    private static IServiceCollection AddSearchByName(this IServiceCollection services)
    {
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

    private static IServiceCollection AddSearchByUpn(this IServiceCollection services)
    {
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

    private static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<
                ISearchServiceAdapter<PupilPremiumLearners, SearchFacets>,
                AzureSearchServiceAdaptor<PupilPremiumLearners, PupilPremiumLearnerDataTransferObject>>()

            .AddSingleton<
                IMapper<Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>>, PupilPremiumLearners>,
                PageablePupilPremiumSearchResultsToLearnerResultsMapper>()

            .AddSingleton<
                IMapperWithResult<PupilPremiumLearnerDataTransferObject, PupilPremiumLearner>,
                PupilPremiumLearnerDataTransferObjectToPupilPremiumLearnerMapper>();
        return services;
    }
}
