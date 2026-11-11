using DfE.GIAP.Core.Downloads.Application.Models.Entries;

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
    public string? Sex { get; set; }
    public DateTime DOB { get; set; }

    public List<CensusAutumnEntry>? CensusAutumn { get; set; }
    public List<CensusSpringEntry>? CensusSpring { get; set; }
    public List<CensusSummerEntry>? CensusSummer { get; set; }
    public List<EarlyYearsFoundationStageProfileEntry>? EarlyYearsFoundationStageProfile { get; set; }
    public List<PhonicsEntry>? Phonics { get; set; }
    public List<KeyStage1Entry>? KeyStage1 { get; set; }
    public List<KeyStage2Entry>? KeyStage2 { get; set; }
    public List<KeyStage4Entry>? KeyStage4 { get; set; }
    public List<MtcEntry>? MTC { get; set; }

    public bool HasCensusAutumnData => CensusAutumn?.Any() ?? false;
    public bool HasCensusSpringData => CensusSpring?.Any() ?? false;
    public bool HasCensusSummerData => CensusSummer?.Any() ?? false;
    public bool HasEYFSPData => EarlyYearsFoundationStageProfile?.Any() ?? false;
    public bool HasPhonicsData => Phonics?.Any() ?? false;
    public bool HasKeyStage1Data => KeyStage1?.Any() ?? false;
    public bool HasKeyStage2Data => KeyStage2?.Any() ?? false;
    public bool HasKeyStage4Data => KeyStage4?.Any() ?? false;
    public bool HasMtcData => MTC?.Any() ?? false;
}
