using System.Diagnostics.CodeAnalysis;
using Azure.Search.Documents;
using DfE.GIAP.Core.MyPupils.Application.Options.Extensions;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Azure.Search.Documents.Models;
using Azure;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services;
internal sealed class TempAggregatePupilsForMyPupilsDomainService : IAggregatePupilsForMyPupilsDomainService
{
    private const int UpnQueryLimit = 4000; // TODO pulled from FA
    private const int PageSplitLimit = 500; // TODO pulled from FA
    private readonly IEnumerable<SearchClient> _searchClients;
    private readonly IMapper<MappableLearnerWithAuthorisationContext, Pupil> _mapper;
    private readonly SearchIndexOptions _searchOptions;

    public TempAggregatePupilsForMyPupilsDomainService(
        IEnumerable<SearchClient> searchClients,
        IMapper<MappableLearnerWithAuthorisationContext, Pupil> mapper,
        IOptions<SearchIndexOptions> searchOptions)
    {
        if (!searchClients.Any())
        {
            throw new ArgumentException("No search clients registered");
        }

        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(searchOptions);
        ArgumentNullException.ThrowIfNull(searchOptions.Value);
        _searchClients = searchClients;
        _mapper = mapper;
        _searchOptions = searchOptions.Value;
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

        SearchClient npdSearchIndex = _searchClients.Single(
            (t) => t.IndexName == _searchOptions.GetIndexOptionsByKey("npd").IndexName);

        SearchClient pupilPremiumSearchClient = _searchClients.Single(
            (t) => t.IndexName == _searchOptions.GetIndexOptionsByKey("pupil-premium").IndexName);

        PaginatedResponse response = new();
        await AddLearnersToResponse(npdSearchIndex, upns, response);
        await AddLearnersToResponse(pupilPremiumSearchClient, upns, response);

        return response.Learners.Select((learner) =>
        {
            return _mapper.Map(
                new MappableLearnerWithAuthorisationContext(learner, authorisationContext));
        });
    }

    private static async Task AddLearnersToResponse(
        SearchClient client,
        IEnumerable<UniquePupilNumber> upns,
        PaginatedResponse response)
    {
        IEnumerable<string> upnValues = upns.Select(t => t.Value);

        if (upnValues.Count() > PageSplitLimit)
        {
            foreach (IEnumerable<string> numbers in SplitNumbers(upnValues.ToArray()))
            {
                await AddLearners(client, response, numbers);
            }
            response.Count = response.Learners.Count;
            return;
        }

        await AddLearners(client, response, upnValues);
    }


    public static async Task AddLearners(
        SearchClient client,
        PaginatedResponse response,
        IEnumerable<string> upns)
    {
        const string Upn = "UPN";

        SearchOptions options = new()
        {
            Size = UpnQueryLimit,
            Skip = 0,
            Filter = upns.Count() > 1 ? $"search.in({Upn}, '{string.Join(",", upns)}')" : $"{Upn} eq '{upns.First()}'",
            IncludeTotalCount = true
        };

        options.SearchFields.Add(Upn);
        options.Select.Add(Upn);
        options.Select.Add("Surname");
        options.Select.Add("Forename");
        options.Select.Add("Middlenames");
        options.Select.Add("Gender");
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
            response.Learners.Add((Learner)result.Document);
        }

        response.Count = results.Value.TotalCount;
    }



    private static List<IEnumerable<string>> SplitNumbers(string[] numbers)
    {
        int numberOfPages = (int)Math.Ceiling((double)numbers.Length / PageSplitLimit);

        if (numberOfPages == 0)
        {
            return [numbers.AsEnumerable()];
        }

        return Enumerable.Range(0, numberOfPages)
            .Select(
                (index) => numbers.Skip(index * PageSplitLimit).Take(PageSplitLimit))
            .ToList();
    }

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

        [JsonProperty("ULN")]
        public string ULN { get; set; }

        [JsonProperty("Surname")]
        public string Surname { get; set; }

        [JsonProperty("Forename")]
        public string Forename { get; set; }

        [JsonProperty("Middlenames")]
        public string Middlenames { get; set; }

        [JsonProperty("Gender")]
        public char? Gender { get; set; }

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
                LearnerNumber = entity.UPN ?? entity.ULN,
                Forename = entity.Forename,
                Surname = entity.Surname,
                MiddleNames = entity.Middlenames,
                Gender = entity.Gender?.ToString() ?? string.Empty,
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
    public class Learner // renamed from Learner to MyLearner because of namespace clash with WEB
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

        /// <summary>
        /// Learners middle names. May be multiple names.
        /// </summary>
        public string MiddleNames { get; set; }

        /// <summary>
        /// Learners gender.
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Learners sex.
        /// </summary>
        public string Sex { get; set; }

        /// <summary>
        /// Learners date of birth.
        /// </summary>
        public DateTime? Dob { get; set; }
    }

    /// <summary>
    /// The paginated search response model
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PaginatedResponse
    {
        /// <summary>
        /// A list of learners that were returned based on a search.
        /// </summary>
        public List<Learner> Learners { get; set; } = new List<Learner>();
        /// <summary>
        /// The list of facets that should be displayed based on the search. May be empty.
        /// </summary>
        public List<FilterData> Filters { get; set; } = new List<FilterData>();
        /// <summary>
        /// The number of learners returned.
        /// </summary>
        public long? Count { get; set; }

        /// <summary>
        /// Converts the list of configured <see cref="Learner"/>
        /// instances to a comma separated string.
        /// </summary>
        /// <returns>
        /// Comma separated string containing the learners defined
        /// withing the ist of configured <see cref="Learner"/> instances.
        /// </returns>
        public string GetLearnerNumbersAsString() =>
            string.Join(",", Learners.ConvertAll(learner => learner.LearnerNumber));
    }

    /// <summary>
    /// Facet group data returned based on a search.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FilterData
    {
        /// <summary>
        /// The name of the facet, i.e. Forename, Surname, Gender
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// List of values for the facet, i.e. first names.
        /// </summary>
        public List<FilterDataItem> Items { get; set; } = new List<FilterDataItem>();
    }

    /// <summary>
    /// Contains specific data about a facet
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FilterDataItem
    {
        /// <summary>
        /// The value of the facet, i.e. a first name.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// how many documents exist that contain the facet, i.e. how many Johns exist.
        /// </summary>
        public long? Count { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

}
