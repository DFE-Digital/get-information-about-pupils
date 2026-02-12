using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Application.Ports;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.Options.Search;
using DfE.GIAP.Core.Search.Application.Options.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByUniquePupilNumber;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Adaptors;
internal sealed class QueryMyPupilsSearchAdaptor : IQueryMyPupilsPort
{
    private readonly ISearchIndexOptionsProvider _searchIndexOptionsProvider;
    private readonly IUseCase<NationalPupilDatabaseSearchByUniquePupilNumberRequest, NationalPupilDatabaseSearchByUniquePupilNumberResponse> _getNpdLearnersUseCase;
    private readonly IMapper<NationalPupilDatabaseLearner, Pupil> _npdLearnerToPupilMapper;
    private readonly IUseCase<PupilPremiumSearchByUniquePupilNumberRequest, PupilPremiumSearchByUniquePupilNumberResponse> _getPupilPremiumLearnersUseCase;
    private readonly IMapper<PupilPremiumLearner, Pupil> _pupilPremiumLearnerToPupilMapper;
    private readonly IMapper<SearchCriteriaOptions, SearchCriteria> _criteriaOptionsToCriteriaMapper;

    public QueryMyPupilsSearchAdaptor(
        ISearchIndexOptionsProvider searchIndexOptionsProvider,
        IUseCase<NationalPupilDatabaseSearchByUniquePupilNumberRequest, NationalPupilDatabaseSearchByUniquePupilNumberResponse> getNpdLearnersUseCase,
        IMapper<NationalPupilDatabaseLearner, Pupil> npdLearnerToPupilMapper,
        IUseCase<PupilPremiumSearchByUniquePupilNumberRequest, PupilPremiumSearchByUniquePupilNumberResponse> getPupilPremiumLearnersUseCase,
        IMapper<PupilPremiumLearner, Pupil> pupilPremiumLearnerToPupilMapper,
        IMapper<SearchCriteriaOptions, SearchCriteria> criteriaOptionsToCriteriaMapper)
    {
        ArgumentNullException.ThrowIfNull(searchIndexOptionsProvider);
        _searchIndexOptionsProvider = searchIndexOptionsProvider;

        ArgumentNullException.ThrowIfNull(getNpdLearnersUseCase);
        _getNpdLearnersUseCase = getNpdLearnersUseCase;

        ArgumentNullException.ThrowIfNull(npdLearnerToPupilMapper);
        _npdLearnerToPupilMapper = npdLearnerToPupilMapper;

        ArgumentNullException.ThrowIfNull(getPupilPremiumLearnersUseCase);
        _getPupilPremiumLearnersUseCase = getPupilPremiumLearnersUseCase;

        ArgumentNullException.ThrowIfNull(pupilPremiumLearnerToPupilMapper);
        _pupilPremiumLearnerToPupilMapper = pupilPremiumLearnerToPupilMapper;

        ArgumentNullException.ThrowIfNull(criteriaOptionsToCriteriaMapper);
        _criteriaOptionsToCriteriaMapper = criteriaOptionsToCriteriaMapper;
    }

    public async Task<IEnumerable<Pupil>> QueryAsync(UniquePupilNumbers myPupils, CancellationToken ctx = default)
    {
        string[] myPupilUniquePupilNumbers = myPupils.GetUniquePupilNumbers().Select(t => t.Value).ToArray();

        SearchIndexOptions npdIndexOptions = _searchIndexOptionsProvider.GetOptions("npd-upn");

        NationalPupilDatabaseSearchByUniquePupilNumberResponse npdSearchResponse =
        await _getNpdLearnersUseCase.HandleRequestAsync(
            new NationalPupilDatabaseSearchByUniquePupilNumberRequest()
            {
                UniquePupilNumbers = myPupilUniquePupilNumbers,
                SearchCriteria = _criteriaOptionsToCriteriaMapper.Map(npdIndexOptions.SearchCriteria!),
                Sort = CreateSortOrder(npdIndexOptions),
                Offset = 0,
            });

        SearchIndexOptions pupilPremiumIndexOptions = _searchIndexOptionsProvider.GetOptions("pupil-premium-upn");

        PupilPremiumSearchByUniquePupilNumberResponse pupilPremiumSearchResponse =
            await _getPupilPremiumLearnersUseCase.HandleRequestAsync(
                new PupilPremiumSearchByUniquePupilNumberRequest()
                {
                    UniquePupilNumbers = myPupilUniquePupilNumbers,
                    SearchCriteria = _criteriaOptionsToCriteriaMapper.Map(pupilPremiumIndexOptions.SearchCriteria!),
                    Sort = CreateSortOrder(pupilPremiumIndexOptions),
                    Offset = 0,
                });

        IEnumerable<Pupil> allPupils =
            (npdSearchResponse.LearnerSearchResults?.Values.Select(_npdLearnerToPupilMapper.Map) ?? [])
                .Concat(pupilPremiumSearchResponse.LearnerSearchResults?.Values.Select(_pupilPremiumLearnerToPupilMapper.Map) ?? [])
                // Deduplicate
                .GroupBy(pupil => pupil.Identifier.Value)
                // Ensure PupilPremium is chosen if a PupilPremium record exists, so display of IsPupilPremium : Yes|No is accurate
                .Select(groupedIdentifiers =>
                    groupedIdentifiers.OrderByDescending(
                        (pupil) => pupil.IsOfPupilType(PupilType.PupilPremium)).First());

        return allPupils;
    }

    private static SortOrder CreateSortOrder(SearchIndexOptions options)
    {
        (string field, string direction) sortOptions = (options.SortOptions ?? new SortOptions()).GetDefaultSort();

        SortOrder sortOrder = new(
           sortField: sortOptions.field,
           sortDirection: sortOptions.direction,
           validSortFields: [sortOptions.field]);

        return sortOrder;
    }
}