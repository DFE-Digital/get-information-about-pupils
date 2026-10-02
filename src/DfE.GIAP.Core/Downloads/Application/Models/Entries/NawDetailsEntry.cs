namespace DfE.GIAP.Core.Downloads.Application.Models.Entries;

public class NawDetailsEntry
{
    public string? SpeakWelsh { get; set; }

    public string? HomeWelsh { get; set; }

    public string? NationalIdentity { get; set; }

    public string? EthnicitySource { get; set; }

    public string? WelshSource { get; set; }

    public string? EALAcquisition { get; set; }

    public string? LanguageSource { get; set; }

    public string? SENCurriculumandTeachingMethods { get; set; }

    public string? SENGroupingandSupport { get; set; }

    public string? SENSpecialisedResources { get; set; }

    public string? SENAdviceandAssessment { get; set; }

    public DateTime? DateEntry { get; set; }

    public bool? FSMTransitionalProtection { get; set; }

    public string? ALNDecisionOutcome { get; set; }

    public string? ALNDecisionBody { get; set; }

    public string? ALNSupportPlan { get; set; }

    public List<string>? ALNAreasOfNeed { get; set; }

    public List<string>? ALNneeds { get; set; }

    public bool? ALNLocalHealthBoardProvision { get; set; }

    public DateTime? IDPReviewDate { get; set; }
}
