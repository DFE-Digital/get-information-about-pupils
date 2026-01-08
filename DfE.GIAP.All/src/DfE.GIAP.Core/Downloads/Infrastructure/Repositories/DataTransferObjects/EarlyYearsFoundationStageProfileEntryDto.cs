using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;

public class EarlyYearsFoundationStageProfileEntryDto
{
    [JsonProperty("FSP_ACADYR")]
    public string? ACADYR { get; set; }

    [JsonProperty("FSP_PUPILMATCHINGREF")]
    public string? PUPILMATCHINGREF { get; set; }

    [JsonProperty("FSP_UPN")]
    public string? UPN { get; set; }

    [JsonProperty("FSP_SURNAME")]
    public string? SURNAME { get; set; }

    [JsonProperty("FSP_FORENAME")]
    public string? FORENAME { get; set; }

    [JsonProperty("FSP_DOB")]
    public string? DOB { get; set; }

    [JsonProperty("FSP_GENDER")]
    public string? GENDER { get; set; }

    [JsonProperty("FSP_SEX")]
    public string? SEX { get; set; }

    [JsonProperty("FSP_MTH_ENTRY")]
    public string? MTH_ENTRY { get; set; }

    [JsonProperty("FSP_LA")]
    public string? LA { get; set; }

    [JsonProperty("FSP_LA_9CODE")]
    public string? LA_9CODE { get; set; }

    [JsonProperty("FSP_ESTAB")]
    public string? ESTAB { get; set; }

    [JsonProperty("FSP_LAESTAB")]
    public string? LAESTAB { get; set; }

    [JsonProperty("FSP_URN")]
    public string? URN { get; set; }

    [JsonProperty("FSP_NFTYPE")]
    public string? NFTYPE { get; set; }

    [JsonProperty("FSP_NPDPUB")]
    public string? NPDPUB { get; set; }

    [JsonProperty("FSP_LLSOA11")]
    public string? LLSOA11 { get; set; }

    [JsonProperty("FSP_IDACISCORE")]
    public string? IDACISCORE { get; set; }

    [JsonProperty("FSP_IDACIRANK")]
    public string? IDACIRANK { get; set; }

    [JsonProperty("FSP_COM_G01")]
    public string? COM_G01 { get; set; }

    [JsonProperty("FSP_COM_G02")]
    public string? COM_G02 { get; set; }

    [JsonProperty("FSP_COM_G03")]
    public string? COM_G03 { get; set; }

    [JsonProperty("FSP_PHY_G04")]
    public string? PHY_G04 { get; set; }

    [JsonProperty("FSP_PHY_G05")]
    public string? PHY_G05 { get; set; }

    [JsonProperty("FSP_PSE_G06")]
    public string? PSE_G06 { get; set; }

    [JsonProperty("FSP_PSE_G07")]
    public string? PSE_G07 { get; set; }

    [JsonProperty("FSP_PSE_G08")]
    public string? PSE_G08 { get; set; }

    [JsonProperty("FSP_LIT_G09")]
    public string? LIT_G09 { get; set; }

    [JsonProperty("FSP_LIT_G10")]
    public string? LIT_G10 { get; set; }

    [JsonProperty("FSP_MAT_G11")]
    public string? MAT_G11 { get; set; }

    [JsonProperty("FSP_MAT_G12")]
    public string? MAT_G12 { get; set; }

    [JsonProperty("FSP_UTW_G13")]
    public string? UTW_G13 { get; set; }

    [JsonProperty("FSP_UTW_G14")]
    public string? UTW_G14 { get; set; }

    [JsonProperty("FSP_UTW_G15")]
    public string? UTW_G15 { get; set; }

    [JsonProperty("FSP_EXP_G16")]
    public string? EXP_G16 { get; set; }

    [JsonProperty("FSP_EXP_G17")]
    public string? EXP_G17 { get; set; }

    [JsonProperty("FSP_COM_E01")]
    public string? COM_E01 { get; set; }

    [JsonProperty("FSP_COM_E02")]
    public string? COM_E02 { get; set; }

    [JsonProperty("FSP_PSE_E03")]
    public string? PSE_E03 { get; set; }

    [JsonProperty("FSP_PSE_E04")]
    public string? PSE_E04 { get; set; }

    [JsonProperty("FSP_PSE_E05")]
    public string? PSE_E05 { get; set; }

    [JsonProperty("FSP_PHY_E06")]
    public string? PHY_E06 { get; set; }

    [JsonProperty("FSP_PHY_E07")]
    public string? PHY_E07 { get; set; }

    [JsonProperty("FSP_LIT_E08")]
    public string? LIT_E08 { get; set; }

    [JsonProperty("FSP_LIT_E09")]
    public string? LIT_E09 { get; set; }

    [JsonProperty("FSP_LIT_E10")]
    public string? LIT_E10 { get; set; }

    [JsonProperty("FSP_MAT_E11")]
    public string? MAT_E11 { get; set; }

    [JsonProperty("FSP_MAT_E12")]
    public string? MAT_E12 { get; set; }

    [JsonProperty("FSP_UTW_E13")]
    public string? UTW_E13 { get; set; }

    [JsonProperty("FSP_UTW_E14")]
    public string? UTW_E14 { get; set; }

    [JsonProperty("FSP_UTW_E15")]
    public string? UTW_E15 { get; set; }

    [JsonProperty("FSP_EXP_E16")]
    public string? EXP_E16 { get; set; }

    [JsonProperty("FSP_EXP_E17")]
    public string? EXP_E17 { get; set; }

    [JsonProperty("FSP_AGE")]
    public string? AGE { get; set; }

    [JsonProperty("FSP_GLD")]
    public string? GLD { get; set; }

    [JsonProperty("FSP_IMD_2010")]
    public string? FSP_IMD_2010 { get; set; }

    [JsonProperty("FSP_IMD_2007")]
    public string? FSP_IMD_2007 { get; set; }

    [JsonProperty("FSP_LLSOA")]
    public string? FSP_LLSOA { get; set; }

    [JsonProperty("FSP_PCON")]
    public string? FSP_PCON { get; set; }

    [JsonProperty("FSP_VERSION")]
    public string? VERSION { get; set; }
}
