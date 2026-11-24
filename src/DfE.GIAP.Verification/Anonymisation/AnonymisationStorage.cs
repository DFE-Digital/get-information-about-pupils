using System.Text.RegularExpressions;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Verification.Sources;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Verification.Anonymisation;

/// <summary>
/// Where the real zips are read from, where the anonymised ones are written to, and the key, with the checks that stop
/// either from being the wrong place.
/// </summary>
/// <remarks>
/// For a real run, they're read from the production Key Vault with the user's Azure CLI login, and are only held in
/// memory. Locally, they can be in configuration, but only if both storage accounts are Azurite.
/// </remarks>
public sealed partial class AnonymisationStorage(IConfiguration configuration, string? keyVaultName)
{
    public const string KeyConfigurationKey = "Verification:AnonymisationKey";

    /// <summary>
    /// A SAS URL for the test environment's <c>verification</c> container, with create and write permissions only.
    /// </summary>
    public const string DestinationConfigurationKey = "Verification:AnonymisedFilesStorage";

    public const string SourceContainerName = "giapdataprocessing";

    public const string DestinationContainerName = "verification";

    /// <summary>
    /// The folder of the <c>verification</c> container the anonymised zips are written to.
    /// </summary>
    public const string FilesFolder = "files";

    private const string AzuriteAccountName = "devstoreaccount1";

    private readonly SecretClient? _secrets = keyVaultName is null
        ? null
        : new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net/"), new AzureCliCredential());

    /// <summary>
    /// The old pipeline's <c>giapdataprocessing</c> container in production. It's only read.
    /// </summary>
    public async Task<BlobContainerClient> GetSourceAsync(CancellationToken cancellationToken = default)
    {
        BlobContainerClient source = CreateContainer(await GetRequiredAsync(LegacyProcessingStorage.ConfigurationKey, cancellationToken), SourceContainerName);

        return source.Name == SourceContainerName
            ? source
            : throw new InvalidOperationException($"The files can only be read from the {SourceContainerName} container, not {source.Name}.");
    }

    /// <summary>
    /// The test environment's <c>verification</c> container, which can only be written to, and the key.
    /// </summary>
    public async Task<(BlobContainerClient Destination, byte[] Key)> GetDestinationAsync(CancellationToken cancellationToken = default)
    {
        BlobContainerClient source = await GetSourceAsync(cancellationToken);
        BlobContainerClient destination = CreateContainer(await GetRequiredAsync(DestinationConfigurationKey, cancellationToken), DestinationContainerName);
        CheckDestination(source, destination, keyFromKeyVault: _secrets is not null);

        return (destination, Convert.FromBase64String(await GetRequiredAsync(KeyConfigurationKey, cancellationToken)));
    }

    /// <summary>
    /// Stops the anonymised files from being written anywhere other than a test environment's <c>verification</c>
    /// container, and a key from configuration from being used with real files.
    /// </summary>
    public static void CheckDestination(BlobContainerClient source, BlobContainerClient destination, bool keyFromKeyVault)
    {
        bool isLocal = source.AccountName == AzuriteAccountName && destination.AccountName == AzuriteAccountName;

        if (destination.Name != DestinationContainerName)
        {
            throw new InvalidOperationException($"The anonymised files can only be written to a {DestinationContainerName} container, not {destination.Name}.");
        }

        if (!isLocal && destination.AccountName == source.AccountName)
        {
            throw new InvalidOperationException("The anonymised files can't be written to the storage account they're read from.");
        }

        if (ProductionIngestAccountRegex().IsMatch(destination.AccountName))
        {
            throw new InvalidOperationException($"The anonymised files can't be written to {destination.AccountName}, which is production's.");
        }

        if (!keyFromKeyVault && !isLocal)
        {
            throw new InvalidOperationException("Real files can only be anonymised with the key in the production Key Vault; use --key-vault.");
        }
    }

    /// <summary>
    /// The old pipeline's folder for an ingest folder.
    /// </summary>
    public static string LegacyFolderName(IngestFolder folder) =>
        folder.Name switch
        {
            "cla-ks2" => "cla/ks2",
            "cla-ks4" => "cla/ks4",
            _ => folder.Name
        };

    // From the Key Vault if there is one, where the secret's name has -- for each :
    private async Task<string> GetRequiredAsync(string key, CancellationToken cancellationToken)
    {
        string? value = _secrets is null
            ? configuration[key]
            : (await _secrets.GetSecretAsync(key.Replace(":", "--", StringComparison.Ordinal), cancellationToken: cancellationToken)).Value.Value;

        return string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException($"{key} isn't configured.") : value;
    }

    // A SAS URL for the container, or locally a connection string
    private static BlobContainerClient CreateContainer(string value, string containerName) =>
        Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && uri.Scheme == Uri.UriSchemeHttps
            ? new BlobContainerClient(uri)
            : new BlobContainerClient(value, containerName);

    // The ingest storage accounts are named <prefix><service><environment>ingestsa, and production's prefix starts s189p
    [GeneratedRegex("^s189p[0-9]+giap[a-z]*ingestsa$")]
    private static partial Regex ProductionIngestAccountRegex();
}
