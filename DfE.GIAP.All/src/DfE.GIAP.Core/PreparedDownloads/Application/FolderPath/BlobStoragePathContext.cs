using DfE.GIAP.Core.PreparedDownloads.Application.Enums;
namespace DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;

public class BlobStoragePathContext
{
    public OrganisationScope OrganisationScope { get; private set; }
    public string UniqueIdentifier { get; private set; } = string.Empty;
    public string LocalAuthorityNumber { get; private set; } = string.Empty;
    public string UniqueReferenceNumber { get; private set; } = string.Empty;

    private BlobStoragePathContext() { }

    public static BlobStoragePathContext Create(
        OrganisationScope organisationScope,
        string? uniqueIdentifier = null,
        string? localAuthorityNumber = null,
        string? uniqueReferenceNumber = null)
    {
        switch (organisationScope)
        {
            case OrganisationScope.AllUsers:
                return new BlobStoragePathContext
                {
                    OrganisationScope = OrganisationScope.AllUsers
                };

            case OrganisationScope.Establishment:
                ArgumentException.ThrowIfNullOrWhiteSpace(uniqueReferenceNumber);
                return new BlobStoragePathContext
                {
                    OrganisationScope = OrganisationScope.Establishment,
                    UniqueReferenceNumber = uniqueReferenceNumber
                };

            case OrganisationScope.LocalAuthority:
                ArgumentException.ThrowIfNullOrWhiteSpace(localAuthorityNumber);
                return new BlobStoragePathContext
                {
                    OrganisationScope = OrganisationScope.LocalAuthority,
                    LocalAuthorityNumber = localAuthorityNumber
                };

            case OrganisationScope.MultiAcademyTrust:
            case OrganisationScope.SingleAcademyTrust:
                ArgumentException.ThrowIfNullOrWhiteSpace(uniqueIdentifier);
                return new BlobStoragePathContext
                {
                    OrganisationScope = organisationScope,
                    UniqueIdentifier = uniqueIdentifier
                };

            default:
                throw new NotImplementedException($"Unhandled OrganisationScope: {organisationScope}");
        }
    }
}

