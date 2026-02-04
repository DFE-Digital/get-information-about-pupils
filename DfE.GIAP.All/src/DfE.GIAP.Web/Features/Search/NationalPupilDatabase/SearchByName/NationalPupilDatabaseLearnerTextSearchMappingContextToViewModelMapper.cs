using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByName;

internal sealed class NationalPupilDatabaseLearnerTextSearchMappingContextToViewModelMapper : IMapper<NationalPupilDatabaseLearnerTextSearchMappingContext, LearnerTextSearchViewModel>
{
    private readonly IMapper<NationalPupilDatabaseLearner, Learner> _npdLearnerToLearnerMapper;

    private readonly IMapper<SearchFacets, List<FilterData>> _filtersResponseMapper;

    public NationalPupilDatabaseLearnerTextSearchMappingContextToViewModelMapper(
        IMapper<NationalPupilDatabaseLearner, Learner> npdLearnerToLearnerMapper,
        IMapper<SearchFacets, List<FilterData>> filtersResponseMapper)
    {
        ArgumentNullException.ThrowIfNull(npdLearnerToLearnerMapper);
        _npdLearnerToLearnerMapper = npdLearnerToLearnerMapper;

        ArgumentNullException.ThrowIfNull(filtersResponseMapper);
        _filtersResponseMapper = filtersResponseMapper;
    }

    public LearnerTextSearchViewModel Map(NationalPupilDatabaseLearnerTextSearchMappingContext input)
    {
        // Map facet filters from the response into structured filter data for the view model.
        input.Model.Filters =
            _filtersResponseMapper.Map(input.Response.FacetedResults);

        // Map each learner from domain to view model using the injected learner mapper.
        List<Learner> learners =
            input.Response.LearnerSearchResults?.Values
                .Select(_npdLearnerToLearnerMapper.Map)
                .ToList() ?? [];

        // Apply PageSize limit
        input.Model.Learners = learners.Take(input.Model.PageSize);

        // Populate meta-data fields for pagination and UI messaging.
        input.Model.Count = input.Response.LearnerSearchResults?.Count ?? 0;
        input.Model.Total = input.Response.TotalNumberOfResults + input.Model.Offset;

        return input.Model;
    }
}
