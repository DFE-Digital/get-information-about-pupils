using DfE.GIAP.Core.PreparedDownloads.Application.Enums;
namespace DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;

public class BlobStoragePathContext
{
    public OrganisationScope OrganisationScope { get; }
    public string Path { get; }

    private BlobStoragePathContext(OrganisationScope scope, string path)
    {
        OrganisationScope = scope;
        Path = path;
    }

    public static BlobStoragePathContext Create(
        OrganisationScope organisationScope,
        string? uniqueIdentifier = null,
        string? localAuthorityNumber = null,
        string? uniqueReferenceNumber = null)
    {
        string path = organisationScope switch
        {
            OrganisationScope.AllUsers => "AllUsers/Metadata/",
            OrganisationScope.Establishment => $"School/{Validate(uniqueReferenceNumber, nameof(uniqueReferenceNumber))}/",
            OrganisationScope.LocalAuthority => $"LA/{Validate(localAuthorityNumber, nameof(localAuthorityNumber))}/",
            OrganisationScope.MultiAcademyTrust => $"MAT/{Validate(uniqueIdentifier, nameof(uniqueIdentifier))}/",
            OrganisationScope.SingleAcademyTrust => $"SAT/{Validate(uniqueIdentifier, nameof(uniqueIdentifier))}/",
            _ => throw new NotImplementedException($"Unhandled OrganisationScope: {organisationScope}")
        };

        return new BlobStoragePathContext(organisationScope, path);
    }

    public string ResolvePath() => Path;

    private static string Validate(string? value, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        return value;
    }
}

