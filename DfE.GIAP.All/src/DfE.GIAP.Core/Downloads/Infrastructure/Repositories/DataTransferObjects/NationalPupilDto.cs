using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;

public class NationalPupilDto
{
    [JsonProperty("UPN")]
    public string? Upn { get; set; }

    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("PupilMatchingRef")]
    public string? PupilMatchingRef { get; set; }

    [JsonProperty("LA")]
    public int LA { get; set; }

    [JsonProperty("Estab")]
    public int Estab { get; set; }

    [JsonProperty("URN")]
    public int Urn { get; set; }

    [JsonProperty("Surname")]
    public string? Surname { get; set; }

    [JsonProperty("Forename")]
    public string? Forename { get; set; }

    [JsonProperty("Middlenames")]
    public string? MiddleName { get; set; }

    [JsonProperty("Gender")]
    public char? Gender { get; set; }

    [JsonProperty("Sex")]
    public char? Sex { get; set; }

    [JsonProperty("DOB")]
    public DateTime DOB { get; set; }

    [JsonProperty("Census_Autumn")]
    public List<CensusAutumnEntryDto> CensusAutumn { get; set; } = new();

    [JsonProperty("Census_Spring")]
    public List<CensusSpringEntryDto> CensusSpring { get; set; } = new();

    [JsonProperty("Census_Summer")]
    public List<CensusSummerEntryDto> CensusSummer { get; set; } = new();

    [JsonProperty("EYFSP")]
    public List<EarlyYearsFoundationStageProfileEntryDto> EarlyYearsFoundationStageProfile { get; set; } = new();

    [JsonProperty("Phonics")]
    public List<PhonicsEntryDto> Phonics { get; set; } = new();

    [JsonProperty("KS1")]
    public List<KeyStage1EntryDto> KeyStage1 { get; set; } = new();

    [JsonProperty("KS2")]
    public List<KeyStage2EntryDto> KeyStage2 { get; set; } = new();

    [JsonProperty("KS4")]
    public List<KeyStage4EntryDto> KeyStage4 { get; set; } = new();

    [JsonProperty("MTC")]
    public List<MtcEntryDto> MTC { get; set; } = new();

    public void SetAdditionalProperties()
    {
        if (CensusSpring != null)
        {
            foreach (CensusSpringEntryDto censusSpringEntity in CensusSpring)
            {
                censusSpringEntity.PupilMatchingRef = PupilMatchingRef;
            }
        }

        if (CensusSummer != null)
        {
            foreach (CensusSummerEntryDto censusSummerEntity in CensusSummer)
            {
                censusSummerEntity.PupilMatchingRef = PupilMatchingRef;
            }
        }

        if (CensusAutumn != null)
        {
            foreach (CensusAutumnEntryDto censusAutumnEntity in CensusAutumn)
            {
                censusAutumnEntity.PupilMatchingRef = PupilMatchingRef;
            }
        }
    }
}
