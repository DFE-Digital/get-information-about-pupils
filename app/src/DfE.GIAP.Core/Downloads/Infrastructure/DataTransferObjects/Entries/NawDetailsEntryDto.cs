using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.DataTransferObjects.Entries;

public class NawDetailsEntryDto
{
    [JsonProperty("SpeakWelsh")]
    public string? SpeakWelsh { get; set; }

    [JsonProperty("HomeWelsh")]
    public string? HomeWelsh { get; set; }

    [JsonProperty("NationalIdentity")]
    public string? NationalIdentity { get; set; }

    [JsonProperty("EthnicitySource")]
    public string? EthnicitySource { get; set; }

    [JsonProperty("WelshSource")]
    public string? WelshSource { get; set; }

    [JsonProperty("EALAcquisition")]
    public string? EALAcquisition { get; set; }

    [JsonProperty("LanguageSource")]
    public string? LanguageSource { get; set; }

    [JsonProperty("SENCurriculumandTeachingMethods")]
    public string? SENCurriculumandTeachingMethods { get; set; }

    [JsonProperty("SENGroupingandSupport")]
    public string? SENGroupingandSupport { get; set; }

    [JsonProperty("SENSpecialisedResources")]
    public string? SENSpecialisedResources { get; set; }

    [JsonProperty("SENAdviceandAssessment")]
    public string? SENAdviceandAssessment { get; set; }

    [JsonProperty("DateEntry")]
    public DateTime? DateEntry { get; set; }

    [JsonProperty("FSMTransitionalProtection")]
    public bool? FSMTransitionalProtection { get; set; }

    [JsonProperty("ALNDecisionOutcome")]
    public string? ALNDecisionOutcome { get; set; }

    [JsonProperty("ALNDecisionBody")]
    public string? ALNDecisionBody { get; set; }

    [JsonProperty("ALNSupportPlan")]
    public string? ALNSupportPlan { get; set; }

    [JsonProperty("ALNAreasOfNeed")]
    public List<string>? ALNAreasOfNeed { get; set; }

    [JsonProperty("ALNneeds")]
    public List<string>? ALNneeds { get; set; }

    [JsonProperty("ALNLocalHealthBoardProvision")]
    public bool? ALNLocalHealthBoardProvision { get; set; }

    [JsonProperty("IDPReviewDate")]
    public DateTime? IDPReviewDate { get; set; }
}
