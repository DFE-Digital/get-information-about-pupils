using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Dto;

#nullable disable
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

    [JsonProperty("Surname")]
    public string Surname { get; set; }

    [JsonProperty("Forename")]
    public string Forename { get; set; }

    [JsonProperty("Sex")]
    public string Sex { get; set; }

    [JsonProperty("DOB")]
    public DateTime? DOB { get; set; }

    [JsonProperty("LocalAuthority")]
    public string LocalAuthority { get; set; }

    [JsonProperty("id")]
    public string id { get; set; }
}
