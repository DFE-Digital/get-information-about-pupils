using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Features.Search.FurtherEducation;
using DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByName;
using DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByUniqueLearnerNumber;
using DfE.GIAP.Web.Features.Search.NationalPupilDatabase;
using DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByName;
using DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.Web.Features.Search.PupilPremium;
using DfE.GIAP.Web.Features.Search.PupilPremium.SearchByName;
using DfE.GIAP.Web.Features.Search.PupilPremium.SearchByUniquePupilNumber;
using DfE.GIAP.Web.Features.Search.SearchOptionsExtensions;
using DfE.GIAP.Web.Features.Search.Shared.Filters;
using DfE.GIAP.Web.Features.Search.Shared.Filters.FilterRegistration;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Handlers;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Mappers;
using DfE.GIAP.Web.Features.Search.Shared.Sort;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search;

public static class CompositionRoot
{
    public static IServiceCollection AddSearch(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSearchCore(configuration);

        services
            .AddSearchOptions(configuration)
            .AddSort()
            .AddFilters();

        services
            .AddNationalPupilDatabaseSearches()
            .AddPupilPremiumSearches()
            .AddFurtherEducationSearches();
            
        return services;
    }



    private static IServiceCollection AddFurtherEducationSearches(this IServiceCollection services)
    {
        services.AddSingleton<
            IMapper<FurtherEducationLearnerTextSearchMappingContext, LearnerTextSearchViewModel>,
            FurtherEducationLearnerTextSearchResponseToViewModelMapper>();

        services.AddSingleton<IMapper<
            FurtherEducationLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>,
            FurtherEducationLearnerNumericSearchResponseToViewModelMapper>();

        services.AddSingleton<IMapper<
            FurtherEducationLearner, Learner>,
            FurtherEducationLearnerToLearnerMapper>();

        return services;
    }

    private static IServiceCollection AddPupilPremiumSearches(this IServiceCollection services)
    {
        services.AddSingleton<
            IMapper<PupilPremiumLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>,
            PupilPremiumLearnerNumericSearchMappingContextToViewModelMapper>();

        services.AddSingleton<
            IMapper<PupilPremiumLearnerTextSearchMappingContext, LearnerTextSearchViewModel>,
            PupilPremiumLearnerTextSearchMappingContextToViewModelMapper>();

        services.AddSingleton<
            IMapper<PupilPremiumLearner, Learner>,
            PupilPremiumLearnerToLearnerMapper>();

        return services;
    }

    private static IServiceCollection AddNationalPupilDatabaseSearches(this IServiceCollection services)
    {
        services.AddSingleton<
            IMapper<NationalPupilDatabaseLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>,
            NationalPupilDatabaseLearnerNumericSearchMappingContextToViewModelMapper>();

        services.AddSingleton<
            IMapper<NationalPupilDatabaseLearnerTextSearchMappingContext, LearnerTextSearchViewModel>,
            NationalPupilDatabaseLearnerTextSearchMappingContextToViewModelMapper>();

        services.AddSingleton<
            IMapper<NationalPupilDatabaseLearner, Learner>,
            NationalPupilDatabaseLearnerToLearnerMapper>();

        return services;
    }

    private static IServiceCollection AddSort(this IServiceCollection services)
    {
        services.AddSingleton<ISortOrderFactory, SortOrderFactory>();
        return services;
    }

    private static IServiceCollection AddFilters(this IServiceCollection services)
    {

        services.AddSingleton<
            IMapper<FilterData, FilterRequest>,
            FilterRequestMapper>();

        services.AddSingleton<
            IMapper<Dictionary<string, string[]>, IList<FilterRequest>>,
            FiltersRequestMapper>();

        services.AddSingleton<
            IMapper<SearchFacet, FilterData>,
            FilterResponseMapper>();

        services.AddSingleton<
            IMapper<SearchFacets, List<FilterData>>,
            FiltersResponseMapper>();

        services.AddSingleton<IFiltersRequestFactory, FiltersRequestFactory>();
        services.AddSingleton<IFilterHandlerRegistry>(_ =>
        {
            Dictionary<string, IFilterHandler> handlers = new()
            {
                { "SurnameLC", new NameFilterHandler("SurnameLC") },
                { "ForenameLC", new NameFilterHandler("ForenameLC") },
                { "MiddlenamesLC", new NameFilterHandler("MiddlenamesLC") },
                { "DOB", new DobFilterHandler() },
                { "Sex", new GenderFilterHandler("Sex") }
            };

            return new FilterHandlerRegistry(handlers);
        });

        return services;
    }
}
