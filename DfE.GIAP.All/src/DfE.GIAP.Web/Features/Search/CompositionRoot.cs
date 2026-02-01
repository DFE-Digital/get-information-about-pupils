using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.SearchRules;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Features.Search.FurtherEducation;
using DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByName;
using DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByUniqueLearnerNumber;
using DfE.GIAP.Web.Features.Search.NationalPupilDatabase;
using DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.Web.Features.Search.Options;
using DfE.GIAP.Web.Features.Search.PupilPremium;
using DfE.GIAP.Web.Features.Search.PupilPremium.SearchByName;
using DfE.GIAP.Web.Features.Search.PupilPremium.SearchByUniquePupilNumber;
using DfE.GIAP.Web.Features.Search.Services;
using DfE.GIAP.Web.Features.Search.Shared.Filters;
using DfE.GIAP.Web.Features.Search.Shared.Filters.FilterRegistration;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Handlers;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Mappers;
using DfE.GIAP.Web.Features.Search.Shared.Sort;
using DfE.GIAP.Web.ViewModels.Search;
using static DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByName.FurtherEducationLearnerTextSearchResponseToViewModelMapper;

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
            .AddSearchRules()
            .AddSort()
            .AddFilters();

        services
            .AddNationalPupilDatabaseSearches()
            .AddPupilPremiumSearches()
            .AddFurtherEducationSearches();
            
        return services;
    }

    private static IServiceCollection AddSearchOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<SearchCriteriaOptions>()
            .Bind(configuration.GetSection(nameof(SearchCriteriaOptions)));

        services
            .AddOptions<SortFieldOptions>()
            .Bind(configuration.GetSection(nameof(SortFieldOptions)));

        services.AddSingleton<ISearchCriteriaProvider, SearchCriteriaProvider>();

        return services;
    }

    private static IServiceCollection AddFurtherEducationSearches(this IServiceCollection services)
    {
        services.AddSingleton<IMapper<
            FurtherEducationLearnerTextSearchMappingContext, LearnerTextSearchViewModel>,
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
        services.AddSingleton<IMapper<
            PupilPremiumLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>,
            PupilPremiumLearnerNumericSearchMappingContextToViewModelMapper>();

        services.AddSingleton<
            IMapper<PupilPremiumLearnerTextSearchMappingContext, LearnerTextSearchViewModel>,
            PupilPremiumLearnerTextSearchMappingContextToViewModelMapper>();

        services.AddSingleton<IMapper<
            PupilPremiumLearner, Learner>,
            PupilPremiumLearnerToLearnerMapper>();

        return services;
    }

    private static IServiceCollection AddNationalPupilDatabaseSearches(this IServiceCollection services)
    {
        services.AddSingleton<IMapper<
            NationalPupilDatabaseLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>,
            NationalPupilDatabaseLearnerNumericSearchMappingContextToViewModelMapper>();

        services.AddSingleton<IMapper<
            NationalPupilDatabaseLearner, Learner>,
            NationalPupilDatabaseLearnerToLearnerMapper>();

        return services;
    }

    private static IServiceCollection AddSort(this IServiceCollection services)
    {
        services.AddSingleton<IMapper<SortOrderRequest, SortOrder>, SortOrderRequestToSortOrderMapper>();
        return services;
    }

    private static IServiceCollection AddSearchRules(this IServiceCollection services)
    {
        services.AddSingleton<ISearchRule, PartialWordMatchRule>();
        return services;
    }

    private static IServiceCollection AddFilters(this IServiceCollection services)
    {

        services.AddSingleton<IMapper<
            FilterData, FilterRequest>,
            FilterRequestMapper>();

        services.AddSingleton<IMapper<
            Dictionary<string, string[]>, IList<FilterRequest>>,
            FiltersRequestMapper>();

        services.AddSingleton<IMapper<
            SearchFacet, FilterData>,
            FilterResponseMapper>();

        services.AddSingleton<IMapper<
            SearchFacets, List<FilterData>>,
            FiltersResponseMapper>();

        services.AddSingleton<IFiltersRequestFactory, FiltersRequestFactory>();
        services.AddSingleton<IFilterHandlerRegistry>(_ =>
        {
            Dictionary<string, IFilterHandler> handlers = new()
            {
                { "SurnameLC", new NameFilterHandler("SurnameLC") },
                { "ForenameLC", new NameFilterHandler("ForenameLC") },
                { "MiddlenamesLC", new NameFilterHandler("MiddlenameLC") },
                { "DOB", new DobFilterHandler() },
                { "Sex", new GenderFilterHandler("Sex") }
            };

            return new FilterHandlerRegistry(handlers);
        });

        return services;
    }
}
