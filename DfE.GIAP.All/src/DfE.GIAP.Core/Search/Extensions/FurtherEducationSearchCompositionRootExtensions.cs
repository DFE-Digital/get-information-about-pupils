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
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.Search.Extensions;
internal static class FurtherEducationSearchCompositionRootExtensions
{

    public static IServiceCollection AddFurtherEducationSearchByName(this IServiceCollection services)
    {
        services.TryAddFurtherEducationSearchInfrastructure();

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

    public static IServiceCollection AddFurtherEducationSearchByUniqueLearnerNumber(this IServiceCollection services)
    {
        services.TryAddFurtherEducationSearchInfrastructure();

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

    private static IServiceCollection TryAddFurtherEducationSearchInfrastructure(this IServiceCollection services)
    {
        services
            .TryAddScoped<
                ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>,
                AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject>>();

        services.TryAddScoped<
                IMapper<
                    Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners>,
                    PageableFurtherEducationSearchResultsToLearnerResultsMapper>();

        services.TryAddSingleton<
                IMapperWithResult<
                    FurtherEducationLearnerDataTransferObject, FurtherEducationLearner>,
                    FurtherEducationSearchResultToLearnerMapper>();

        return services;
    }
}
