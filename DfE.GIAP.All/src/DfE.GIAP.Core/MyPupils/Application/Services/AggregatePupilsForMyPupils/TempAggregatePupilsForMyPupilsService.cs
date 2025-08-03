using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Search.Provider;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
internal sealed class TempAggregatePupilsForMyPupilsApplicationService : IAggregatePupilsForMyPupilsApplicationService
{
    private const int UpnQueryLimit = 4000; // TODO pulled from FA
    private const int DefaultPageSize = 20; // the maximum pupils returned for any query
    private readonly ISearchClientProvider _searchClientProvider;
    private readonly IMapper<DecoratedSearchIndexDto, Pupil> _mapper;

    public TempAggregatePupilsForMyPupilsApplicationService(
        ISearchClientProvider searchClientProvider,
        IMapper<DecoratedSearchIndexDto, Pupil> mapper)
    {

        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(searchClientProvider);
        _searchClientProvider = searchClientProvider;
        _mapper = mapper;
    }


    public async Task<IEnumerable<Pupil>> GetPupilsAsync(
        IEnumerable<UniquePupilNumber> uniquePupilNumbers,
        MyPupilsQueryOptions? queryOptions = null)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(uniquePupilNumbers.Count(), UpnQueryLimit);

        if (!uniquePupilNumbers.Any())
        {
            return [];
        }

        MyPupilsQueryOptions validatedQueryOptions = queryOptions ?? MyPupilsQueryOptions.Default();

        int skip = DefaultPageSize * (validatedQueryOptions.Page.Value - 1);

        List<UniquePupilNumber> pagedUpns =
            uniquePupilNumbers
                .Skip(skip)
                .Take(DefaultPageSize)
                .ToList();

        List<DecoratedSearchIndexDto> npdResults =
            await SearchLearners(
                _searchClientProvider.GetClientByKey("npd"),
                pagedUpns,
                validatedQueryOptions,
                PupilType.NationalPupilDatabase);

        if (npdResults.Count >= DefaultPageSize) // if npd reaches page size then no need to call pupil-premium
        {
            return npdResults.Select( _mapper.Map);
        }

        List<DecoratedSearchIndexDto> ppResults =
            await SearchLearners(
                _searchClientProvider.GetClientByKey("pupil-premium"),
                pagedUpns,
                validatedQueryOptions,
                PupilType.PupilPremium);

        return npdResults
            .Concat(ppResults)
            .DistinctBy(t => t.SearchIndexDto.UPN) // Deduplicate between indexes
            .Take(DefaultPageSize)
            .Select(_mapper.Map);
    }


    internal static async Task<List<DecoratedSearchIndexDto>> SearchLearners(
        SearchClient client,
        IEnumerable<UniquePupilNumber> upns,
        MyPupilsQueryOptions? queryOptions,
        PupilType pupilType)
    {
        MyPupilsQueryOptions validatedOptions = queryOptions ?? MyPupilsQueryOptions.Default();

        const string UpnIndexField = "UPN";

        List<AzureIndexEntity> output = [];

        string filter = upns.Count() > 1
            ? $"search.in({UpnIndexField}, '{string.Join(",", upns.Select(t => t.Value))}')"
            : $"UPN eq '{upns.First()}'";

        SearchOptions options = new()
        {
            Size = DefaultPageSize,
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

        string sortField = validatedOptions.Order.Field switch
        {
            "Forename" => "Forename",
            "Surname" => "Surname",
            "Sex" => "Sex",
            "DOB" => "DOB",
            _ => "search.score()" // If unknown field is passed
        };

        string sortDirection = validatedOptions.Order.Direction switch
        {
            SortDirection.Ascending => "asc",
            _ => "desc"
        };

        options.OrderBy.Add($"{sortField} {sortDirection}");

        Response<SearchResults<AzureIndexEntity>> results = await client.SearchAsync<AzureIndexEntity>("*", options);

        await foreach (SearchResult<AzureIndexEntity> result in results.Value.GetResultsAsync())
        {
            output.Add(result.Document);
        }

        return output.Select(indexDto => new DecoratedSearchIndexDto(indexDto, pupilType)).ToList();
    }
}
