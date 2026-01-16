using Newtonsoft.Json;

namespace DfE.GIAP.Web.Features.Auth.Infrastructure.DataTransferObjects;
#nullable enable
public class OrganisationDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    [JsonProperty("name")]
    public string? Name { get; set; }
    [JsonProperty("category")]
    public OrganisationCategoryDto? Category { get; set; }
    [JsonProperty("type")]
    public EstablishmentTypeDto? EstablishmentType { get; set; }
    [JsonProperty("statutoryLowAge")]
    public string? StatutoryLowAge { get; set; }
    [JsonProperty("statutoryHighAge")]
    public string? StatutoryHighAge { get; set; }
    [JsonProperty("establishmentNumber")]
    public string? EstablishmentNumber { get; set; }
    [JsonProperty("localAuthority")]
    public LocalAuthorityDto? LocalAuthority { get; set; }
    [JsonProperty("urn")]
    public string? UniqueReferenceNumber { get; set; }
    [JsonProperty("uid")]
    public string? UniqueIdentifier { get; set; }
    [JsonProperty("ukprn")]
    public string? UKProviderReferenceNumber { get; set; }
}

public class OrganisationCategoryDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
}

public class EstablishmentTypeDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
}

public class LocalAuthorityDto
{
    [JsonProperty("code")]
    public string Code { get; set; } = string.Empty;
}
