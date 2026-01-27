using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.SearchRules;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Learner.FurtherEducation;
using DfE.GIAP.Core.Search.Application.Models.Learner.PupilPremium;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Features.Search.FurtherEducation;
using DfE.GIAP.Web.Features.Search.PupilPremium;
using DfE.GIAP.Web.Features.Search.Shared.Filters;
using DfE.GIAP.Web.Features.Search.Shared.Filters.FilterRegistration;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Handlers;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Mappers;
using DfE.GIAP.Web.ViewModels.Search;
using static DfE.GIAP.Web.Features.Search.FurtherEducation.FurtherEducationLearnerTextSearchResponseToViewModelMapper;

namespace DfE.GIAP.Web.Features.Search;

public static class CompositionRoot
{
    public static IServiceCollection AddSearch(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSearchDependencies(configuration);

        services
            .AddSearchRules()
            .AddFilters();

        services
            .AddFurtherEducationSearches()
            .AddPupilPremiumSearches();

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
            FurtherEducationLearnerToViewModelMapper>();

        return services;
    }

    private static IServiceCollection AddPupilPremiumSearches(this IServiceCollection services)
    {
        services.AddSingleton<IMapper<
            PupilPremiumLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>,
            PupilPremiumLearnerNumericSearchResponseToViewModelMapper>();

        services.AddSingleton<IMapper<
            PupilPremiumLearner, Learner>,
            PupilPremiumLearnerToViewModelMapper>();

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
                { "DOB", new DobFilterHandler() },
                { "Sex", new GenderFilterHandler("Sex") }
            };

            return new FilterHandlerRegistry(handlers);
        });

        return services;
    }
}
