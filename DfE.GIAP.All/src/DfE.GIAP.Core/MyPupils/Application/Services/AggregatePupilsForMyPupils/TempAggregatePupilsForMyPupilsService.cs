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
    private const int PageSplitLimit = 500; // TODO pulled from FA
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
        if (!uniquePupilNumbers.Any())
        {
            return [];
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(uniquePupilNumbers.Count(), UpnQueryLimit);
        MyPupilsQueryOptions validatedOptions = queryOptions ?? MyPupilsQueryOptions.Default();

        const int DefaultPageSize = 20;

        IEnumerable<DecoratedSearchIndexDto> npdLearners =
            (await SearchForLearnersByUpn(
                    client: _searchClientProvider.GetClientByKey(name: "npd"),
                    uniquePupilNumbers,
                    validatedOptions))
                .Select((npdSearchIndexDto)
                    => new DecoratedSearchIndexDto(
                        npdSearchIndexDto,
                        PupilType.NationalPupilDatabase));

        // TODO extension on DecoratedLearnerMap and reuse
        if (npdLearners.Count() == DefaultPageSize)
        {
            return npdLearners.Select(
                decoratedLearner =>
                    _mapper.Map(
                        new DecoratedSearchIndexDto(
                            decoratedLearner.SearchIndexDto,
                            decoratedLearner.PupilType)));
        }

        // else fetch more from pp

        IEnumerable<DecoratedSearchIndexDto> pupilPremiumLearners =
            (await SearchForLearnersByUpn(
                    client: _searchClientProvider.GetClientByKey(name: "pupil-premium"),
                    uniquePupilNumbers,
                    validatedOptions))
                .Select(
                    (pupilPremiumSearchIndexDto) => new DecoratedSearchIndexDto(
                        pupilPremiumSearchIndexDto,
                        PupilType.PupilPremium));

        return npdLearners.Concat(pupilPremiumLearners)
            .Take(DefaultPageSize)
            .Select(
                (decoratedLearner) =>
                    _mapper.Map(
                        new DecoratedSearchIndexDto(
                            decoratedLearner.SearchIndexDto,
                            decoratedLearner.PupilType)));
    }



    private static async Task<List<AzureIndexEntity>> SearchForLearnersByUpn(
        SearchClient client,
        IEnumerable<UniquePupilNumber> upns,
        MyPupilsQueryOptions options)
    {
        IEnumerable<string> upnValues = upns.Select(t => t.Value);

        if (upnValues.Count() <= PageSplitLimit)
        {
            return await SearchLearners(client, upnValues, options);
        }

        List<AzureIndexEntity> learners = [];

        foreach (IEnumerable<string> upnsSplitPart in SplitUpnsToFitPagingLimit(upns))
        {
            List<AzureIndexEntity> learnersToAdd = await SearchLearners(client, upnsSplitPart, options);
            learners.AddRange(learnersToAdd);
        }

        return learners;
    }


    private static async Task<List<AzureIndexEntity>> SearchLearners(
        SearchClient client,
        IEnumerable<string> upns,
        MyPupilsQueryOptions queryOptions)
    {
        const string UpnIndexField = "UPN";

        List<AzureIndexEntity> output = [];

        // e.g Skip and Size act as a Take
        // - page 1 with pageSize 20 -> 20 * (1-1) = 0-19 results
        // - page 3 with pageSize 20 -> 20 * (3-1) = skips the first 40 (0-39) -> 40-59 results
        const int PageSize = 20;
        int resultsToSkip = PageSize * (queryOptions.Page.Value - 1);

        SearchOptions options = new()
        {
            Size = PageSize,
            Skip = resultsToSkip,
            Filter = upns.Count() > 1 ? $"search.in({UpnIndexField}, '{string.Join(",", upns)}')" : $"{UpnIndexField} eq '{upns.First()}'",
            IncludeTotalCount = true
        };

        options.SearchFields.Add(UpnIndexField);
        options.Select.Add(UpnIndexField);
        options.Select.Add("Surname");
        options.Select.Add("Forename");
        options.Select.Add("Sex");
        options.Select.Add("DOB");
        options.Select.Add("LocalAuthority");
        options.Select.Add("id");

        string sortField = queryOptions.Order.Field switch
        {
            "Forename" => "Forename",
            "Surname" => "Surname",
            "Sex" => "Sex",
            "DOB" => "DOB",
            _ => "search.score()" // If unknown field is passed 1-1 mapping otherwise
        };

        string sortDirection = queryOptions.Order.Direction switch
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

        return output;
    }

    internal static List<IEnumerable<string>> SplitUpnsToFitPagingLimit(IEnumerable<UniquePupilNumber> upns)
    {
        IEnumerable<string> upnValues = upns.Select(t => t.Value);

        if (!upnValues.Any())
        {
            return [];
        }

        int numberOfPages = (int)Math.Ceiling((double)upnValues.Count() / PageSplitLimit);

        if (numberOfPages == 0)
        {
            return [upnValues];
        }

        return Enumerable.Range(0, numberOfPages)
            .Select((index)
                => upnValues.Skip(index * PageSplitLimit).Take(PageSplitLimit))
            .ToList();
    }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}
