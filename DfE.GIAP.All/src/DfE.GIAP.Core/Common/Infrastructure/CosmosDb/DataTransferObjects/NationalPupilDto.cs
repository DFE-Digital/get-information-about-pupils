using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects.Entries;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;

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
    public List<CensusAutumnEntryDto>? CensusAutumn { get; set; }

    [JsonProperty("Census_Spring")]
    public List<CensusSpringEntryDto>? CensusSpring { get; set; }

    [JsonProperty("Census_Summer")]
    public List<CensusSummerEntryDto>? CensusSummer { get; set; }

    [JsonProperty("EYFSP")]
    public List<EarlyYearsFoundationStageProfileEntryDto>? EarlyYearsFoundationStageProfile { get; set; }

    [JsonProperty("Phonics")]
    public List<PhonicsEntryDto>? Phonics { get; set; }

    [JsonProperty("KS1")]
    public List<KeyStage1EntryDto>? KeyStage1 { get; set; }

    [JsonProperty("KS2")]
    public List<KeyStage2EntryDto>? KeyStage2 { get; set; }

    [JsonProperty("KS4")]
    public List<KeyStage4EntryDto>? KeyStage4 { get; set; }

    [JsonProperty("MTC")]
    public List<MtcEntryDto>? MTC { get; set; }
}
