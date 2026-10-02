using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.DataTransferObjects.Entries;

public class NawDetailsEntryDto
{
    [JsonProperty(nameof(SpeakWelsh))]
    public string? SpeakWelsh { get; set; }

    [JsonProperty(nameof(HomeWelsh))]
    public string? HomeWelsh { get; set; }

    [JsonProperty(nameof(NationalIdentity))]
    public string? NationalIdentity { get; set; }

    [JsonProperty(nameof(EthnicitySource))]
    public string? EthnicitySource { get; set; }

    [JsonProperty(nameof(WelshSource))]
    public string? WelshSource { get; set; }

    [JsonProperty(nameof(EALAcquisition))]
    public string? EALAcquisition { get; set; }

    [JsonProperty(nameof(LanguageSource))]
    public string? LanguageSource { get; set; }

    [JsonProperty(nameof(SENCurriculumandTeachingMethods))]
    public string? SENCurriculumandTeachingMethods { get; set; }

    [JsonProperty(nameof(SENGroupingandSupport))]
    public string? SENGroupingandSupport { get; set; }

    [JsonProperty(nameof(SENSpecialisedResources))]
    public string? SENSpecialisedResources { get; set; }

    [JsonProperty(nameof(SENAdviceandAssessment))]
    public string? SENAdviceandAssessment { get; set; }

    [JsonProperty(nameof(DateEntry))]
    public DateTime? DateEntry { get; set; }

    [JsonProperty(nameof(FSMTransitionalProtection))]
    public bool? FSMTransitionalProtection { get; set; }

    [JsonProperty(nameof(ALNDecisionOutcome))]
    public string? ALNDecisionOutcome { get; set; }

    [JsonProperty(nameof(ALNDecisionBody))]
    public string? ALNDecisionBody { get; set; }

    [JsonProperty(nameof(ALNSupportPlan))]
    public string? ALNSupportPlan { get; set; }

    [JsonProperty(nameof(ALNAreasOfNeed))]
    public List<string>? ALNAreasOfNeed { get; set; }

    [JsonProperty(nameof(ALNneeds))]
    public List<string>? ALNneeds { get; set; }

    [JsonProperty(nameof(ALNLocalHealthBoardProvision))]
    public bool? ALNLocalHealthBoardProvision { get; set; }

    [JsonProperty(nameof(IDPReviewDate))]
    public DateTime? IDPReviewDate { get; set; }
}
