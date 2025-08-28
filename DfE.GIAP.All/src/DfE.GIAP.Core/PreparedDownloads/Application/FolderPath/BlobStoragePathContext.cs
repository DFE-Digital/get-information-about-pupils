using DfE.GIAP.Core.PreparedDownloads.Application.Enums;
namespace DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;

public class BlobStoragePathContext
{
    public OrganisationScope OrganisationScope { get; }
    public string? UniqueIdentifier { get; }
    public string? LocalAuthorityNumber { get; }
    public string? UniqueReferenceNumber { get; }

    private BlobStoragePathContext(
        OrganisationScope organisationScope,
        string? uniqueIdentifier,
        string? localAuthorityNumber,
        string? uniqueReferenceNumber)
    {
        OrganisationScope = organisationScope;
        UniqueIdentifier = uniqueIdentifier;
        LocalAuthorityNumber = localAuthorityNumber;
        UniqueReferenceNumber = uniqueReferenceNumber;
    }

    public static BlobStoragePathContext Create(
        OrganisationScope organisationScope,
        string? uniqueIdentifier = null,
        string? localAuthorityNumber = null,
        string? uniqueReferenceNumber = null)
    {
        return organisationScope switch
        {
            OrganisationScope.AllUsers =>
                new BlobStoragePathContext(organisationScope, null, null, null),
            OrganisationScope.Establishment =>
                new BlobStoragePathContext(organisationScope, null, null, Validate(uniqueReferenceNumber, nameof(uniqueReferenceNumber))),
            OrganisationScope.LocalAuthority =>
                new BlobStoragePathContext(organisationScope, null, Validate(localAuthorityNumber, nameof(localAuthorityNumber)), null),
            OrganisationScope.MultiAcademyTrust or OrganisationScope.SingleAcademyTrust =>
                new BlobStoragePathContext(organisationScope, Validate(uniqueIdentifier, nameof(uniqueIdentifier)), null, null),
            _ => throw new NotImplementedException($"Unhandled OrganisationScope: {organisationScope}")
        };
    }

    public string ResolvePath()
    {
        return OrganisationScope switch
        {
            OrganisationScope.Establishment =>
                $"School/{UniqueReferenceNumber}/",
            OrganisationScope.LocalAuthority =>
                $"LA/{LocalAuthorityNumber}/",
            OrganisationScope.MultiAcademyTrust =>
                $"MAT/{UniqueIdentifier}/",
            OrganisationScope.SingleAcademyTrust =>
                $"SAT/{UniqueIdentifier}/",
            OrganisationScope.AllUsers =>
                "AllUsers/Metadata/",
            _ => throw new NotImplementedException($"Unhandled OrganisationScope: {OrganisationScope}")
        };
    }

    private static string Validate(string? value, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        return value!;
    }
}


