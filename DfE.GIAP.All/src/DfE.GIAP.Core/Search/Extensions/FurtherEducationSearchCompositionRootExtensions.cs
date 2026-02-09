using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByUniqueLearnerNumber;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.Mappers;
using DfE.GIAP.Core.Search.Infrastructure.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Search.Extensions;
internal static class FurtherEducationSearchCompositionRootExtensions
{
    internal static IServiceCollection AddFurtherEducationSearch(this IServiceCollection services)
    {
        services
            .AddSearchByName()
            .AddSearchByUpn()
            .AddInfrastructure();

        return services;
    }

    private static IServiceCollection AddSearchByName(this IServiceCollection services)
    {
        services
            .AddScoped<
                IUseCase<
                    FurtherEducationSearchByNameRequest, FurtherEducationSearchByNameResponse>,
                    FurtherEducationSearchByNameUseCase>()

            .AddScoped<
                ISearchLearnerByNameService<FurtherEducationLearners>,
                SearchLearnerByNameService<FurtherEducationLearners>>();

        return services;
    }

    private static IServiceCollection AddSearchByUpn(this IServiceCollection services)
    {
        services
            .AddScoped<
                IUseCase<
                    FurtherEducationSearchByUniqueLearnerNumberRequest, FurtherEducationSearchByUniqueLearnerNumberResponse>,
                    FurtherEducationSearchByUniqueLearnerNumberUseCase>()

            .AddScoped<
                ISearchLearnersByIdentifierService<FurtherEducationLearners>,
                SearchLearnerByIdentifierService<FurtherEducationLearners>>();

        return services;
    }

    private static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<
                ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>,
                AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject>>()

            .AddSingleton<
                IMapper<
                    Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners>,
                    PageableFurtherEducationSearchResultsToLearnerResultsMapper>()

            .AddSingleton<
                IMapperWithResult<
                    FurtherEducationLearnerDataTransferObject, FurtherEducationLearner>,
                    FurtherEducationSearchResultToLearnerMapper>();

        return services;
    }
}
