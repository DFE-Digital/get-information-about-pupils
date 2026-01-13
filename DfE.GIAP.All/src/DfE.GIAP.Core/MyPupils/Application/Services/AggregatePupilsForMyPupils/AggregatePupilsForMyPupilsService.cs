using Azure.Search.Documents;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Search;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
// TODO this COULD be replaced with a CosmosDb implementation to avoid what it previously used - AzureSearch
internal sealed class AggregatePupilsForMyPupilsApplicationService : IAggregatePupilsForMyPupilsApplicationService
{
    private const int UpnQueryLimit = 4000; // TODO pulled from FA
    private readonly ISearchClientProvider _searchClientProvider;
    private readonly IMapper<AzureIndexEntityWithPupilType, Pupil> _mapper;

    public AggregatePupilsForMyPupilsApplicationService(
        ISearchClientProvider searchClientProvider,
        IMapper<AzureIndexEntityWithPupilType, Pupil> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(searchClientProvider);
        _searchClientProvider = searchClientProvider;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Pupil>> GetPupilsAsync(UniquePupilNumbers uniquePupilNumbers)
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

        List<Pupil> distinctResults = allResults
            // Deduplicate
            .GroupBy(p => p.SearchIndexDto.UPN)
            // Ensure PupilPremium is chosen if a PupilPremium record exists, so display of IsPupilPremium : Yes|No is accurate
            .Select(g =>
                g.OrderByDescending(x => x.PupilType == PupilType.PupilPremium)
                 .First())
            // Explicit display order: NPD first, then PP
            .OrderBy(p => p.PupilType == PupilType.PupilPremium ? 1 : 0)
            .Select(_mapper.Map)
            .ToList();


        return distinctResults;
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
