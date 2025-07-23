using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Client;
using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Mapper;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService;

internal sealed class TempAggregatePupilsForMyPupilsDomainService : IAggregatePupilsForMyPupilsDomainService
{
    private const int UpnQueryLimit = 4000; // TODO pulled from FA
    private const int PageSplitLimit = 500; // TODO pulled from FA
    private readonly ISearchClientProvider _searchClientProvider;
    private readonly IMapper<MappableLearnerWithAuthorisationContext, Pupil> _mapper;


    public TempAggregatePupilsForMyPupilsDomainService(
        ISearchClientProvider searchClientProvider,
        IMapper<MappableLearnerWithAuthorisationContext, Pupil> mapper)
    {

        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(searchClientProvider);
        _searchClientProvider = searchClientProvider;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Pupil>> GetPupilsAsync(
        IEnumerable<PupilIdentifier> pupilIdentifiers,
        PupilAuthorisationContext authorisationContext,
        PupilSelectionDomainCriteria pupilSelectionCriteria)
    {
        if(pupilIdentifiers.Count() == 0)
        {
            return [];
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(pupilIdentifiers.Count(), UpnQueryLimit);
        ArgumentNullException.ThrowIfNull(authorisationContext);
        ArgumentNullException.ThrowIfNull(pupilSelectionCriteria);


        Dictionary<string, PupilId> upnToPupilIdMap =
            pupilIdentifiers.ToDictionary(
                (pupilIdentifier) => pupilIdentifier.UniquePupilNumber.Value,
                (pupilIdentifier) => pupilIdentifier.PupilId);

        IEnumerable<DecoratedLearnerWithPupilType> npdLearners =
            (await SearchForLearnersByUpn(
                    client: _searchClientProvider.GetClientByKey(name: "npd"),
                    pupilIdentifiers.Select(t => t.UniquePupilNumber),
                    pupilSelectionCriteria))
                .Select((npdLearner)
                    => new DecoratedLearnerWithPupilType(npdLearner, PupilType.NationalPupilDatabase));


        IEnumerable<DecoratedLearnerWithPupilType> pupilPremiumLearners =
            (await SearchForLearnersByUpn(
                    client: _searchClientProvider.GetClientByKey(name: "pupil-premium"),
                    pupilIdentifiers.Select(t => t.UniquePupilNumber),
                    pupilSelectionCriteria))
                .Select(t => new DecoratedLearnerWithPupilType(t, PupilType.PupilPremium));

        return npdLearners.Concat(pupilPremiumLearners)
            .Take(pupilSelectionCriteria.Count)
            .Select(decoratedLearner =>
                _mapper.Map(
                    new MappableLearnerWithAuthorisationContext(
                        upnToPupilIdMap[decoratedLearner.Learner.UPN],
                        decoratedLearner.Learner,
                        decoratedLearner.PupilType,
                        authorisationContext)));
    }



    private static async Task<List<Learner>> SearchForLearnersByUpn(
        SearchClient client,
        IEnumerable<UniquePupilNumber> upns,
        PupilSelectionDomainCriteria pupilSelectionCriteria)
    {
        IEnumerable<string> upnValues = upns.Select(t => t.Value);

        if (upnValues.Count() <= PageSplitLimit)
        {
            return await SearchLearners(client, upnValues, pupilSelectionCriteria);
        }

        List<Learner> learners = [];

        foreach (IEnumerable<string> upnsSplitPart in SplitUpnsToFitPagingLimit(upns))
        {
            List<Learner> learnersToAdd = await SearchLearners(client, upnsSplitPart, pupilSelectionCriteria);
            learners.AddRange(learnersToAdd);
        }

        return learners;
    }


    public static async Task<List<Learner>> SearchLearners(
        SearchClient client,
        IEnumerable<string> upns,
        PupilSelectionDomainCriteria pupilSelectionCriteria)
    {
        const string UpnIndexField = "UPN";

        List<Learner> output = [];

        // e.g Skip and Size act as a Take
        // - page 1 with pageSize 20 -> 20 * (1-1) = 0-19 results
        // - page 3 with pageSize 20 -> 20 * (3-1) = skips the first 40 (0-39) -> 40-59 results 
        int resultsToSkip = pupilSelectionCriteria.Count * (pupilSelectionCriteria.Page - 1);

        SearchOptions options = new()
        {
            Size = pupilSelectionCriteria.Count,
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

        string sortField = pupilSelectionCriteria.SortBy switch
        {
            "Forename" => "Forename",
            "Surname" => "Surname",
            "Sex" => "Sex",
            "DOB" => "DOB",
            _ => "search.score()" // If unknown field is passed 1-1 mapping otherwise
        };

        string sortDirection = pupilSelectionCriteria.Direction switch
        {
            SortDirection.Ascending => "asc",
            _ => "desc"
        };

        options.OrderBy.Add($"{sortField} {sortDirection}");

        Response<SearchResults<AzureIndexEntity>> results = await client.SearchAsync<AzureIndexEntity>("*", options);

        await foreach (SearchResult<AzureIndexEntity> result in results.Value.GetResultsAsync())
        {
            output.Add((Learner)result.Document);
        }

        return output;
    }

    private static List<IEnumerable<string>> SplitUpnsToFitPagingLimit(IEnumerable<UniquePupilNumber> upns)
    {
        IEnumerable<string> upnValues = upns.Select(t => t.Value);
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

    private sealed record DecoratedLearnerWithPupilType(Learner Learner, PupilType PupilType);
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}
