using System.Text.Json;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Verification.Anonymisation;

/// <summary>
/// A real zip to anonymise, and where the anonymised one goes.
/// </summary>
/// <param name="Source">Its path in the old pipeline's <c>giapdataprocessing</c> container, e.g. <c>ks2/processed/NPD_KS2_2025_U.zip</c>.</param>
/// <param name="Target">Its ingest file path, e.g. <c>ks2/NPD_KS2_2025_U.zip</c>, which is also its path in the <c>files</c> folder of the <c>verification</c> container.</param>
public sealed record ManifestFile(string Source, string Target)
{
    public IngestFolder Folder => IngestFolder.ParseFilePath(Target).Folder;
}

/// <summary>
/// The zips the pipelines are verified with, in the order they're uploaded.
/// </summary>
public sealed record AnonymisationManifest(IReadOnlyList<ManifestFile> Files)
{
    public const string FileName = "manifest.json";

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static AnonymisationManifest Load(string path) => Parse(File.ReadAllText(path));

    public static AnonymisationManifest Parse(string json)
    {
        AnonymisationManifest manifest = JsonSerializer.Deserialize<AnonymisationManifest>(json, _jsonOptions)
            ?? throw new FormatException("The manifest is empty.");

        if (manifest.Files is not { Count: > 0 })
        {
            throw new FormatException("The manifest has no files.");
        }

        foreach (ManifestFile file in manifest.Files)
        {
            (IngestFolder folder, string zipName) = IngestFolder.ParseFilePath(file.Target);
            string legacyFolder = AnonymisationStorage.LegacyFolderName(folder);

            if (!zipName.StartsWith(folder.ZipNamePrefix, StringComparison.Ordinal) || zipName.Contains('/'))
            {
                throw new FormatException($"'{file.Target}' isn't a zip the {folder.Name} folder accepts.");
            }

            if (!file.Source.StartsWith($"{legacyFolder}/raw/", StringComparison.Ordinal)
                && !file.Source.StartsWith($"{legacyFolder}/processed/", StringComparison.Ordinal))
            {
                throw new FormatException($"'{file.Source}' isn't in the old pipeline's {legacyFolder}/raw/ or {legacyFolder}/processed/ folder.");
            }
        }

        string[] duplicates = [.. manifest.Files.GroupBy(f => f.Target).Where(g => g.Count() > 1).Select(g => g.Key)];
        if (duplicates.Length > 0)
        {
            throw new FormatException($"The manifest has these targets more than once: {string.Join(", ", duplicates)}.");
        }

        return manifest;
    }

    public string ToJson() => JsonSerializer.Serialize(this, _jsonOptions);
}
