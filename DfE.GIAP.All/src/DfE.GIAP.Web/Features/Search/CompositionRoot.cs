using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.SearchRules;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Learner.FurtherEducation;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Controllers.LearnerNumber.Mappers;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.FilterRegistration;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.Handlers;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;
using DfE.GIAP.Web.ViewModels.Search;
using static DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers.FurtherEducationLearnerTextSearchResponseToViewModelMapper;
using DfE.GIAP.Core.Search;

namespace DfE.GIAP.Web.Features.Search;

public static class CompositionRoot
{
    public static IServiceCollection AddSearch(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSearchDependencies(configuration);

        services.AddSingleton<IMapper<
            LearnerTextSearchMappingContext, LearnerTextSearchViewModel>,
            FurtherEducationLearnerTextSearchResponseToViewModelMapper>();

        services.AddSingleton<IMapper<
            FurtherEducationLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>,
            FurtherEducationLearnerNumericSearchResponseToViewModelMapper>();

        services.AddSingleton<IMapper<
            FurtherEducationLearner, Learner>, LearnerToViewModelMapper>();

        services.AddSingleton<IMapper<SortOrderRequest, SortOrder>, SortOrderMapper>();

        services.AddSingleton<ISearchRule, PartialWordMatchRule>();

        services.AddFilters();

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
