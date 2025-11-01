namespace DfE.GIAP.Core.Downloads.Application.Models;

public class NationalPupil
{
    public string? Upn { get; set; }
    public string? Id { get; set; }
    public string? PupilMatchingRef { get; set; }
    public int LA { get; set; }
    public int Estab { get; set; }
    public int Urn { get; set; }
    public string? Surname { get; set; }
    public string? Forename { get; set; }
    public string? MiddleName { get; set; }
    public char? Gender { get; set; }
    public char? Sex { get; set; }
    public DateTime DOB { get; set; }

    public List<CensusAutumnEntry> CensusAutumn { get; set; } = new();
    public List<CensusSpringEntry> CensusSpring { get; set; } = new();
    public List<CensusSummerEntry> CensusSummer { get; set; } = new();
    public List<EarlyYearsFoundationStageProfileEntry> EarlyYearsFoundationStageProfile { get; set; } = new();
    public List<PhonicsEntry> Phonics { get; set; } = new();
    public List<KeyStage1Entry> KeyStage1 { get; set; } = new();
    public List<KeyStage2Entry> KeyStage2 { get; set; } = new();
    public List<KeyStage4Entry> KeyStage4 { get; set; } = new();
    public List<MtcEntry> MTC { get; set; } = new();

    public bool HasCensusAutumData => CensusAutumn.Any();
    public bool HasCensusSpringData => CensusSpring.Any();
    public bool HasCensusSummerData => CensusSummer.Any();
    public bool HasEYFSPData => EarlyYearsFoundationStageProfile.Any();
    public bool HasPhonicsData => Phonics.Any();
    public bool HasKeyStage1Data => KeyStage1.Any();
    public bool HasKeyStage2Data => KeyStage2.Any();
    public bool HasKeyStage4Data => KeyStage4.Any();
    public bool HasMtcData => MTC.Any();

    public void SetAdditionalProperties()
    {
        if (CensusSpring != null)
        {
            foreach (CensusSpringEntry censusSpringEntity in CensusSpring)
            {
                censusSpringEntity.PupilMatchingRef = PupilMatchingRef;
            }
        }

        if (CensusSummer != null)
        {
            foreach (CensusSummerEntry censusSummerEntity in CensusSummer)
            {
                censusSummerEntity.PupilMatchingRef = PupilMatchingRef;
            }
        }

        if (CensusAutumn != null)
        {
            foreach (CensusAutumnEntry censusAutumnEntity in CensusAutumn)
            {
                censusAutumnEntity.PupilMatchingRef = PupilMatchingRef;
            }
        }
    }
}
