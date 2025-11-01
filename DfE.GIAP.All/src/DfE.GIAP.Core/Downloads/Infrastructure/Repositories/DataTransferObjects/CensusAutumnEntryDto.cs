﻿using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;

public class CensusAutumnEntryDto
{
    public string? PupilMatchingRef { get; set; }

    [JsonProperty("AcademicYear")]
    public string? AcademicYear { get; set; }

    [JsonProperty("CensusTerm")]
    public string? CensusTerm { get; set; }

    [JsonProperty("LA")]
    public string? LocalAuthority { get; set; }

    [JsonProperty("Estab")]
    public string? Establishment { get; set; }

    [JsonProperty("LAEstab")]
    public string? LocalAuthorityEstablishment { get; set; }

    [JsonProperty("URN")]
    public string? UniqueReferenceNumber { get; set; }

    [JsonProperty("Phase")]
    public string? Phase { get; set; }

    [JsonProperty("UPN")]
    public string? UniquePupilNumber { get; set; }

    [JsonProperty("FormerUPN")]
    public string? FormerUniquePupilNumber { get; set; }

    [JsonProperty("Surname")]
    public string? Surname { get; set; }

    [JsonProperty("Forename")]
    public string? Forename { get; set; }

    [JsonProperty("MiddleNames")]
    public string? MiddleNames { get; set; }

    [JsonProperty("PreferredSurname")]
    public string? PreferredSurname { get; set; }

    [JsonProperty("FormerSurname")]
    public string? FormerSurname { get; set; }

    [JsonProperty("Gender")]
    public string? Gender { get; set; }

    [JsonProperty("Sex")]
    public string? Sex { get; set; }

    [JsonProperty("DOB")]
    public string? DOB { get; set; }

    [JsonProperty("FSMeligible")]
    public string? FreeSchoolMealEligible { get; set; }

    [JsonProperty("FSM_Protected")]
    public string? FreeSchoolMealProtected { get; set; }

    [JsonProperty("Language")]
    public string? Language { get; set; }

    [JsonProperty("HoursAtSetting")]
    public string? HoursAtSetting { get; set; }

    [JsonProperty("FundedHours")]
    public string? FundedHours { get; set; }

    [JsonProperty("EnrolStatus")]
    public string? EnrolStatus { get; set; }

    [JsonProperty("EntryDate")]
    public string? EntryDate { get; set; }

    [JsonProperty("NCyearActual")]
    public string? NationalCurriculumYearActual { get; set; }

    [JsonProperty("SENprovision")]
    public string? SpecialEducationalNeedsProvision { get; set; }

    [JsonProperty("PrimarySENtype")]
    public string? PrimarySpecialEducationalNeedsType { get; set; }

    [JsonProperty("SecondarySENtype")]
    public string? SecondarySpecialEducationalNeedsType { get; set; }

    [JsonProperty("IDACI_S")]
    public string? IncomeDeprivationAffectingChildrenIndexScore { get; set; }

    [JsonProperty("IDACI_R")]
    public string? IncomeDeprivationAffectingChildrenIndexRating { get; set; }

    [JsonProperty("ExtendedHours")]
    public string? ExtendedHours { get; set; }

    [JsonProperty("ExpandedHours")]
    public string? ExpandedHours { get; set; }

    [JsonProperty("DAFIndicator")]
    public int DisabilityAccessFundIndicator { get; set; }

    [JsonProperty("TLevelNonqualHrs")]
    public string? TLevelNonqualHrs { get; set; }

    [JsonProperty("TLevelQualHrs")]
    public string? TLevelQualHrs { get; set; }
}
