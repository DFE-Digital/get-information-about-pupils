namespace DfE.GIAP.Web.Features.Auth.Application.Models;

/// <summary>
/// Organisation details returned from the identity provider.
/// </summary>
public class Organisation
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public OrganisationCategory? Category { get; set; }
    public EstablishmentType? EstablishmentType { get; set; }
    public string? StatutoryLowAge { get; set; }
    public string? StatutoryHighAge { get; set; }
    public string? EstablishmentNumber { get; set; }
    public LocalAuthority? LocalAuthority { get; set; }
    public string? UniqueReferenceNumber { get; set; }
    public string? UniqueIdentifier { get; set; }
    public string? UKProviderReferenceNumber { get; set; }
}

public class OrganisationCategory
{
    public string Id { get; set; } = string.Empty;
}

public class EstablishmentType
{
    public string Id { get; set; } = string.Empty;
}

public class LocalAuthority
{
    public string Code { get; set; } = string.Empty;
}
