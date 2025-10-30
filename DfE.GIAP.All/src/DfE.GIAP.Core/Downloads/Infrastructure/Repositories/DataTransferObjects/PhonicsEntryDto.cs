using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;

public class PhonicsEntryDto
{
    [JsonProperty("Phonics_ACADYR")]
    public string? AcademicYear { get; set; }

    [JsonProperty("Phonics_PUPILMATCHINGREF")]
    public string? PupilMatchingReference { get; set; }

    [JsonProperty("Phonics_id")]
    public string? Id { get; set; }

    [JsonProperty("Phonics_UPN")]
    public string? UniquePupilNumber { get; set; }

    [JsonProperty("Phonics_SURNAME")]
    public string? SurName { get; set; }

    [JsonProperty("Phonics_FORENAMES")]
    public string? ForeNames { get; set; }

    [JsonProperty("Phonics_GENDER")]
    public string? Gender { get; set; }

    [JsonProperty("Phonics_SEX")]
    public string? SEX { get; set; }

    [JsonProperty("Phonics_DOB")]
    public string? DateOfBirth { get; set; }

    [JsonProperty("Phonics_LA")]
    public string? LocalAuthority { get; set; }

    [JsonProperty("Phonics_ESTAB")]
    public string? Establishment { get; set; }

    [JsonProperty("Phonics_URN")]
    public string? UniqueReferenceNumber { get; set; }

    [JsonProperty("Phonics_NCYEARACTUAL")]
    public string? NationalCurriculumYearActual { get; set; }

    [JsonProperty("Phonics_TOE_CODE")]
    public string? TypeOfEstablishmentCode { get; set; }

    [JsonProperty("Phonics_PHONICS_MARK")]
    public string? Phonics_Mark { get; set; }

    [JsonProperty("Phonics_PHONICS_OUTCOME")]
    public string? Phonics_Outcome { get; set; }

    [JsonProperty("Phonics_VERSION")]
    public string? Version { get; set; }

    [JsonProperty("Phonics_PHONICS_MARK_AUT21")]
    public string? Phonics_Mark_Aut21 { get; set; }

    [JsonProperty("Phonics_PHONICS_OUTCOME_AUT21")]
    public string? Phonics_Outcome_Aut21 { get; set; }
}
