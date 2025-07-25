using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Dto;

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
            Id = entity.id,
            UPN = entity.UPN ?? string.Empty,
            Forename = entity.Forename,
            Surname = entity.Surname,
            Sex = entity.Sex?.ToString() ?? string.Empty,
            Dob = entity.DOB,
            LocalAuthority = entity.LocalAuthority
        };
    }
}
