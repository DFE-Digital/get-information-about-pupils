using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Handlers;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByUniquePupilNumber;
using DfE.GIAP.Web.Features.Search.Options.Search;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
// TODO this COULD be replaced with a CosmosDb implementation to avoid what it previously used - AzureSearch
internal sealed class AggregatePupilsForMyPupilsApplicationService : IAggregatePupilsForMyPupilsApplicationService
{
    private const int UpnQueryLimit = 4000;
    private readonly ISearchIndexOptionsProvider _searchIndexOptionsProvider;
    private readonly IUseCase<NationalPupilDatabaseSearchByUniquePupilNumberRequest, NationalPupilDatabaseSearchByUniquePupilNumberResponse> _npdSearchServiceAdaptor;
    private readonly IUseCase<PupilPremiumSearchByUniquePupilNumberRequest, PupilPremiumSearchByUniquePupilNumberResponse> _pupilPremiumSearchServiceAdaptor;
    private readonly IMapper<NationalPupilDatabaseLearner, Pupil> _npdLearnerToPupilMapper;
    private readonly IMapper<PupilPremiumLearner, Pupil> _pupilPremiumLearnerToPupilMapper;
    private readonly IOrderPupilsHandler _orderPupilsHandler;
    private readonly IPaginatePupilsHandler _paginatePupilsHandler;
    private readonly IMapper<SearchCriteriaOptions, SearchCriteria> _criteriaOptionsToCriteriaMapper;

    public AggregatePupilsForMyPupilsApplicationService(
        ISearchIndexOptionsProvider searchIndexOptionsProvider,
        IUseCase<NationalPupilDatabaseSearchByUniquePupilNumberRequest, NationalPupilDatabaseSearchByUniquePupilNumberResponse> getNpdLearnersUseCase,
        IMapper<NationalPupilDatabaseLearner, Pupil> npdLearnerToPupilMapper,
        IUseCase<PupilPremiumSearchByUniquePupilNumberRequest, PupilPremiumSearchByUniquePupilNumberResponse> getPupilPremiumLearnersUseCase,
        IMapper<PupilPremiumLearner, Pupil> pupilPremiumLearnerToPupilMapper,
        IOrderPupilsHandler orderPupilsHandler,
        IPaginatePupilsHandler paginatePupilsHandler,
        IMapper<SearchCriteriaOptions, SearchCriteria> criteriaOptionsToCriteriaMapper)
    {
        ArgumentNullException.ThrowIfNull(searchIndexOptionsProvider);
        _searchIndexOptionsProvider = searchIndexOptionsProvider;

        ArgumentNullException.ThrowIfNull(getNpdLearnersUseCase);
        _npdSearchServiceAdaptor = getNpdLearnersUseCase;

        ArgumentNullException.ThrowIfNull(npdLearnerToPupilMapper);
        _npdLearnerToPupilMapper = npdLearnerToPupilMapper;
        
        ArgumentNullException.ThrowIfNull(getPupilPremiumLearnersUseCase);
        _pupilPremiumSearchServiceAdaptor = getPupilPremiumLearnersUseCase;

        ArgumentNullException.ThrowIfNull(pupilPremiumLearnerToPupilMapper);

        _pupilPremiumLearnerToPupilMapper = pupilPremiumLearnerToPupilMapper;

        ArgumentNullException.ThrowIfNull(orderPupilsHandler);
        _orderPupilsHandler = orderPupilsHandler;

        ArgumentNullException.ThrowIfNull(paginatePupilsHandler);
        _paginatePupilsHandler = paginatePupilsHandler;

        ArgumentNullException.ThrowIfNull(criteriaOptionsToCriteriaMapper);
        _criteriaOptionsToCriteriaMapper = criteriaOptionsToCriteriaMapper;
    }

    public async Task<IEnumerable<Pupil>> GetPupilsAsync(
        UniquePupilNumbers uniquePupilNumbers,
        MyPupilsQueryModel? query = null,
        CancellationToken ctx = default)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(uniquePupilNumbers.Count, UpnQueryLimit);

        if (uniquePupilNumbers.IsEmpty)
        {
            return [];
        }

        const string defaultSort = "search.score()";

        SortOrder sortOrder = new(
            sortField: defaultSort,
            sortDirection: "desc",
            validSortFields: [defaultSort]);

        string[] myPupilUniquePupilNumbers = uniquePupilNumbers.GetUniquePupilNumbers().Select(t => t.Value).ToArray();

        SearchIndexOptions npdIndexOptions = _searchIndexOptionsProvider.GetOptions("npd-upn");

        List<Pupil> outputPupils = [];

        NationalPupilDatabaseSearchByUniquePupilNumberResponse searchResponse =
            await _npdSearchServiceAdaptor.HandleRequestAsync(
                new NationalPupilDatabaseSearchByUniquePupilNumberRequest()
                {
                    UniquePupilNumbers = myPupilUniquePupilNumbers,
                    SearchCriteria = _criteriaOptionsToCriteriaMapper.Map(npdIndexOptions.SearchCriteria!),
                    Offset = 0,
                    Sort = sortOrder
                });

        SearchIndexOptions pupilPremiumIndexOptions = _searchIndexOptionsProvider.GetOptions("pupil-premium-upn");

        PupilPremiumSearchByUniquePupilNumberResponse pupilPremiumSearchResponse =
            await _pupilPremiumSearchServiceAdaptor.HandleRequestAsync(
                new PupilPremiumSearchByUniquePupilNumberRequest()
                {
                    UniquePupilNumbers = myPupilUniquePupilNumbers,
                    SearchCriteria = _criteriaOptionsToCriteriaMapper.Map(pupilPremiumIndexOptions.SearchCriteria!),
                    Sort = sortOrder,
                    Offset = 0,
                });

        IEnumerable<Pupil> allPupils =
            (searchResponse.LearnerSearchResults?.Values.Select(_npdLearnerToPupilMapper.Map) ?? [])
                .Concat(pupilPremiumSearchResponse.LearnerSearchResults?.Values.Select(_pupilPremiumLearnerToPupilMapper.Map) ?? [])
                // Deduplicate
                .GroupBy(pupil => pupil.Identifier.Value)
                // Ensure PupilPremium is chosen if a PupilPremium record exists, so display of IsPupilPremium : Yes|No is accurate
                .Select(groupedIdentifiers  =>
                    groupedIdentifiers.OrderByDescending(
                        (pupil) => pupil.IsOfPupilType(PupilType.PupilPremium)).First());

        // If no query, return ALL results
        if (query is null)
        {
            return allPupils;
        }

        // Order, then paginate

        IEnumerable<Pupil> orderedPupils = _orderPupilsHandler.Order(allPupils, query.Order);

        return _paginatePupilsHandler.PaginatePupils(orderedPupils, query.PaginateOptions);
    }
}
