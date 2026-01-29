using Newtonsoft.Json;

namespace DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects.Entries;

public class KeyStage1EntryDto
{
    [JsonProperty("ACADYR")]
    public string? ACADYR { get; set; }

    [JsonProperty("PUPILMATCHINGREF")]
    public string? PUPILMATCHINGREF { get; set; }

    [JsonProperty("KS1_ID")]
    public string? KS1_ID { get; set; }

    [JsonProperty("UPN")]
    public string? UPN { get; set; }

    [JsonProperty("SURNAME")]
    public string? SURNAME { get; set; }

    [JsonProperty("FORENAMES")]
    public string? FORENAMES { get; set; }

    [JsonProperty("DOB")]
    public DateTime? DOB { get; set; }

    [JsonProperty("GENDER")]
    public string? GENDER { get; set; }

    [JsonProperty("SEX")]
    public string? SEX { get; set; }

    [JsonProperty("LA")]
    public string? LA { get; set; }

    [JsonProperty("LA_9Code")]
    public string? LA_9Code { get; set; }

    [JsonProperty("ESTAB")]
    public string? ESTAB { get; set; }

    [JsonProperty("LAESTAB")]
    public string? LAESTAB { get; set; }

    [JsonProperty("URN")]
    public string? URN { get; set; }

    [JsonProperty("ToE_CODE")]
    public string? ToE_CODE { get; set; }

    [JsonProperty("MMSCH")]
    public string? MMSCH { get; set; }

    [JsonProperty("MMSCH2")]
    public string? MMSCH2 { get; set; }

    [JsonProperty("MSCH")]
    public string? MSCH { get; set; }

    [JsonProperty("MSCH2")]
    public string? MSCH2 { get; set; }

    [JsonProperty("MOB1")]
    public string? MOB1 { get; set; }

    [JsonProperty("MOB2")]
    public string? MOB2 { get; set; }

    [JsonProperty("DISC_READ")]
    public string? DISC_READ { get; set; }

    [JsonProperty("DISC_WRIT")]
    public string? DISC_WRIT { get; set; }

    [JsonProperty("DISC_MAT")]
    public string? DISC_MAT { get; set; }

    [JsonProperty("DISC_SCI")]
    public string? DISC_SCI { get; set; }

    [JsonProperty("DISC_PSENG")]
    public string? DISC_PSENG { get; set; }

    [JsonProperty("DISC_PSREAD")]
    public string? DISC_PSREAD { get; set; }

    [JsonProperty("DISC_PSWRITE")]
    public string? DISC_PSWRITE { get; set; }

    [JsonProperty("DISC_PSSPEAK")]
    public string? DISC_PSSPEAK { get; set; }

    [JsonProperty("DISC_PSLISTEN")]
    public string? DISC_PSLISTEN { get; set; }

    [JsonProperty("DISC_PSMATHS")]
    public string? DISC_PSMATHS { get; set; }

    [JsonProperty("DISC_PSNUM")]
    public string? DISC_PSNUM { get; set; }

    [JsonProperty("DISC_PSUSING")]
    public string? DISC_PSUSING { get; set; }

    [JsonProperty("DISC_PSSHAPE")]
    public string? DISC_PSSHAPE { get; set; }

    [JsonProperty("DISC_PSSCIENCE")]
    public string? DISC_PSSCIENCE { get; set; }

    [JsonProperty("READ_OUTCOME")]
    public string? READ_OUTCOME { get; set; }

    [JsonProperty("WRIT_OUTCOME")]
    public string? WRIT_OUTCOME { get; set; }

    [JsonProperty("MATH_OUTCOME")]
    public string? MATH_OUTCOME { get; set; }

    [JsonProperty("SCI_OUTCOME")]
    public string? SCI_OUTCOME { get; set; }

    [JsonProperty("PSENG")]
    public string? PSENG { get; set; }

    [JsonProperty("PSREAD")]
    public string? PSREAD { get; set; }

    [JsonProperty("PSWRITE")]
    public string? PSWRITE { get; set; }

    [JsonProperty("PSSPEAK")]
    public string? PSSPEAK { get; set; }

    [JsonProperty("PSLISTEN")]
    public string? PSLISTEN { get; set; }

    [JsonProperty("PSMATHS")]
    public string? PSMATHS { get; set; }

    [JsonProperty("PSNUM")]
    public string? PSNUM { get; set; }

    [JsonProperty("PSUSING")]
    public string? PSUSING { get; set; }

    [JsonProperty("PSSHAPE")]
    public string? PSSHAPE { get; set; }

    [JsonProperty("PSSCIENCE")]
    public string? PSSCIENCE { get; set; }

    [JsonProperty("NPDPUB")]
    public string? NPDPUB { get; set; }

    [JsonProperty("NFTYPE")]
    public string? NFTYPE { get; set; }

    [JsonProperty("SPEAKANDLISTEN")]
    public string? SPEAKANDLISTEN { get; set; }

    [JsonProperty("APS")]
    public string? APS { get; set; }

    [JsonProperty("READING")]
    public string? READING { get; set; }

    [JsonProperty("WRITING")]
    public string? WRITING { get; set; }

    [JsonProperty("READWRIT")]
    public string? READWRIT { get; set; }

    [JsonProperty("MATHS")]
    public string? MATHS { get; set; }

    [JsonProperty("SCIEXPINVEST")]
    public string? SCIEXPINVEST { get; set; }

    [JsonProperty("SCILIFPROT")]
    public string? SCILIFPROT { get; set; }

    [JsonProperty("SCIMATPROP")]
    public string? SCIMATPROP { get; set; }

    [JsonProperty("SCIPHYSPROC")]
    public string? SCIPHYSPROC { get; set; }

    [JsonProperty("SCIENCE")]
    public string? SCIENCE { get; set; }

    [JsonProperty("VERSION")]
    public string? VERSION { get; set; }
}
