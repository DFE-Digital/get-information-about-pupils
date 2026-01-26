using Azure.Search.Documents;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Handlers;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Search;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
// TODO this COULD be replaced with a CosmosDb implementation to avoid what it previously used - AzureSearch
internal sealed class AggregatePupilsForMyPupilsApplicationService : IAggregatePupilsForMyPupilsApplicationService
{
    private const int UpnQueryLimit = 4000; // TODO pulled from FA
    private readonly ISearchClientProvider _searchClientProvider;
    private readonly IMapper<AzureIndexEntityWithPupilType, Pupil> _dtoToEntityMapper;
    private readonly IOrderPupilsHandler _orderPupilsHandler;
    private readonly IPaginatePupilsHandler _paginatePupilsHandler;

    public AggregatePupilsForMyPupilsApplicationService(
        ISearchClientProvider searchClientProvider,
        IMapper<AzureIndexEntityWithPupilType, Pupil> dtoToEntityMapper,
        IOrderPupilsHandler orderPupilsHandler,
        IPaginatePupilsHandler paginatePupilsHandler)
    {
        ArgumentNullException.ThrowIfNull(searchClientProvider);
        _searchClientProvider = searchClientProvider;

        ArgumentNullException.ThrowIfNull(dtoToEntityMapper);
        _dtoToEntityMapper = dtoToEntityMapper;

        ArgumentNullException.ThrowIfNull(orderPupilsHandler);
        _orderPupilsHandler = orderPupilsHandler;

        ArgumentNullException.ThrowIfNull(paginatePupilsHandler);
        _paginatePupilsHandler = paginatePupilsHandler;
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

        List<AzureIndexEntityWithPupilType> allResults = [];

        const int maxIndexQuerySize = 500;
        foreach (UniquePupilNumber[] upnBatch in uniquePupilNumbers.GetUniquePupilNumbers().Chunk(maxIndexQuerySize))
        {
            SearchOptions searchOptions = CreateSearchClientOptions(upnBatch);

            IEnumerable<AzureIndexEntityWithPupilType> npdResults =
                (await _searchClientProvider.InvokeSearchAsync<AzureIndexEntity>("npd", searchOptions))
                    .ToDecoratedSearchIndexDto(PupilType.NationalPupilDatabase);

            IEnumerable<AzureIndexEntityWithPupilType> ppResults =
                (await _searchClientProvider.InvokeSearchAsync<AzureIndexEntity>("pupil-premium", searchOptions))
                    .ToDecoratedSearchIndexDto(PupilType.PupilPremium);

            allResults.AddRange(npdResults);
            allResults.AddRange(ppResults);
        }

        IEnumerable<Pupil> distinctResults = allResults
            // Deduplicate
            .GroupBy(p => p.SearchIndexDto.UPN)
            // Ensure PupilPremium is chosen if a PupilPremium record exists, so display of IsPupilPremium : Yes|No is accurate
            .Select(groupedByUpn =>
                groupedByUpn.OrderByDescending(x => x.PupilType == PupilType.PupilPremium).First())
            .Select(_dtoToEntityMapper.Map);

        // If no query, return ALL results
        if(query is null)
        {
            return distinctResults;
        }

        // Order, then paginate
        return
            _paginatePupilsHandler.PaginatePupils(
                _orderPupilsHandler.Order(distinctResults, query.Order), query.PaginateOptions);
    }


    internal static SearchOptions CreateSearchClientOptions(IEnumerable<UniquePupilNumber> upns)
    {
        const string UpnIndexField = "UPN";

        string filter = upns.Count() > 1
            ? $"search.in({UpnIndexField}, '{string.Join(",", upns.Select(t => t.Value))}')"
            : $"UPN eq '{upns.First()}'";

        SearchOptions options = new()
        {
            Filter = filter
        };

        options.SearchFields.Add(UpnIndexField);
        options.Select.Add(UpnIndexField);
        options.Select.Add("Surname");
        options.Select.Add("Forename");
        options.Select.Add("Sex");
        options.Select.Add("DOB");
        options.Select.Add("LocalAuthority");
        options.Select.Add("id");
        //options.OrderBy.Add($"{UpnIndexField} asc"); // is score deterministic enough?

        return options;
    }
}
