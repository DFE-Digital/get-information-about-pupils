using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
#nullable disable
/// <summary>
/// Models the data as existing in the cognitive search indexes. Allows us to map UPN/ULN to LearnerNumber in Learner
/// </summary>
[ExcludeFromCodeCoverage]
public class AzureIndexEntity
{
    [JsonProperty("@search.score")]
    public string Score { get; set; }

    public string UPN { get; set; }

    public string Surname { get; set; }

    public string Forename { get; set; }

    public string Sex { get; set; }

    public DateTime? DOB { get; set; }

    public string LocalAuthority { get; set; }

    public string id { get; set; }
}
