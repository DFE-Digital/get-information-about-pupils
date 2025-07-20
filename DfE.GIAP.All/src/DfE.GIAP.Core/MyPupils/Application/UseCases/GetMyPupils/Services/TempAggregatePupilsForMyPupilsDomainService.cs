using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.Client;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services;

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
        IEnumerable<UniquePupilNumber> upns,
        PupilAuthorisationContext authorisationContext)
    {

#pragma warning disable S125 // Sections of code should not be commented out
        //if (request.PageSize < 1)
        //    throw new ArgumentException("page size must be above 1");
        //if (request.PageNumber < 0)
        //    throw new ArgumentException("page number must be above 0");

#pragma warning restore S125 // Sections of code should not be commented out

        if (upns.Count() > UpnQueryLimit)
        {
            throw new ArgumentException("UPN limit exceeded");
        }

        IEnumerable<DecoratedLearnerWithPupilType> npdLearners =
            (await SearchForLearnersByUpn(
                    client: _searchClientProvider.GetClientByKey(key: "npd"),
                    upns))
                .Select((npdLearner)
                    => new DecoratedLearnerWithPupilType(npdLearner, PupilType.NationalPupilDatabase));


        IEnumerable<DecoratedLearnerWithPupilType> pupilPremiumLearners =
            (await SearchForLearnersByUpn(
                    client: _searchClientProvider.GetClientByKey(key: "pupil-premium"),
                    upns))
                .Select(t => new DecoratedLearnerWithPupilType(t, PupilType.PupilPremium));

        return npdLearners.Concat(pupilPremiumLearners)
            .Select(decoratedLearner =>
                _mapper.Map(
                    new MappableLearnerWithAuthorisationContext(
                        decoratedLearner.Learner,
                        decoratedLearner.PupilType,
                        authorisationContext)));
    }



    private static async Task<List<Learner>> SearchForLearnersByUpn(
        SearchClient client,
        IEnumerable<UniquePupilNumber> upns)
    {
        IEnumerable<string> upnValues = upns.Select(t => t.Value);

        if (upnValues.Count() <= PageSplitLimit)
        {
            return await SearchLearners(client, upnValues);
        }

        List<Learner> learners = [];

        foreach (IEnumerable<string> upnsSplitPart in SplitUpnsToFitPagingLimit(upns))
        {
            List<Learner> learnersToAdd = await SearchLearners(client, upnsSplitPart);
            learners.AddRange(learnersToAdd);
        }

        return learners;
    }


    public static async Task<List<Learner>> SearchLearners(
        SearchClient client,
        IEnumerable<string> upns)
    {
        const string UpnIndexField = "UPN";

        List<Learner> output = [];

        SearchOptions options = new()
        {
            Size = UpnQueryLimit,
            Skip = 0,
            Filter = upns.Count() > 1 ? $"search.in({UpnIndexField}, '{string.Join(",", upns)}')" : $"{UpnIndexField} eq '{upns.First()}'",
            IncludeTotalCount = true
        };

        options.SearchFields.Add(UpnIndexField);
        options.Select.Add(UpnIndexField);
        options.Select.Add("Surname");
        options.Select.Add("Forename");
        // options.Select.Add("Middlenames");
        // options.Select.Add("Gender");
        options.Select.Add("Sex");
        options.Select.Add("DOB");
        options.Select.Add("LocalAuthority");
        options.Select.Add("id");

        string requestSortField = "search.score()";
        string requestSortDirection = "desc";

        options.OrderBy.Add($"{requestSortField} {requestSortDirection}");

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

    /// <summary>
    /// Models the data as existing in the cognitive search indexes. Allows us to map UPN/ULN to LearnerNumber in Learner
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AzureIndexEntity
    {
        [JsonProperty("@search.score")]
        public string Score { get; set; }

        [JsonProperty("UPN")]
        public string UPN { get; set; }

        //[JsonProperty("ULN")]
        //public string ULN { get; set; }

        [JsonProperty("Surname")]
        public string Surname { get; set; }

        [JsonProperty("Forename")]
        public string Forename { get; set; }

        //[JsonProperty("Middlenames")]
        //public string Middlenames { get; set; }

        //[JsonProperty("Gender")]
        //public char? Gender { get; set; }

        [JsonProperty("Sex")]
        public char? Sex { get; set; }

        [JsonProperty("DOB")]
        public DateTime? DOB { get; set; }

        [JsonProperty("LocalAuthority")]
        public string LocalAuthority { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        public static explicit operator Learner(AzureIndexEntity entity)
        {
            return new Learner()
            {
                LearnerNumber = entity.UPN ?? string.Empty,
                Forename = entity.Forename,
                Surname = entity.Surname,
                Sex = entity.Sex?.ToString() ?? string.Empty,
                Dob = entity.DOB,
                Id = entity.id,
                LocalAuthority = entity.LocalAuthority
            };
        }
    }

    /// <summary>
    /// Data about learners that is stored in the cognitive search index. Returned based on searches.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Learner
    {
        /// <summary>
        /// Cognitive search ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Learner number - may be UPN or ULN.
        /// </summary>
        public string LearnerNumber { get; set; }

        /// <summary>
        /// The local authority code for the learner.
        /// </summary>
        public string LocalAuthority { get; set; }

        /// <summary>
        /// Learners surname
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Learners forename. May be multiple names
        /// </summary>
        public string Forename { get; set; }

        // TODO UNUSED SO COMMENTED OUT
        ///// <summary>
        ///// Learners middle names. May be multiple names.
        ///// </summary>
        //public string MiddleNames { get; set; }


        // TODO UNUSED SO COMMENTED OUT
        ///// <summary>
        ///// Learners gender.
        ///// </summary>
        //public string Gender { get; set; }

        /// <summary>
        /// Learners sex.
        /// </summary>
        public string Sex { get; set; }

        /// <summary>
        /// Learners date of birth.
        /// </summary>
        public DateTime? Dob { get; set; }
    }

}
