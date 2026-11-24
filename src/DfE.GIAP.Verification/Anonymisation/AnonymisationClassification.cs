using System.Text.Json;
using System.Text.Json.Serialization;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Verification.Anonymisation;

/// <summary>
/// A PupilMatchingRef format and the reserved range its values are permuted into.
/// </summary>
public sealed record PmrFormat(IdentifierFormat Source, IdentifierFormat Target);

/// <summary>
/// What's done to each column of a folder's data files.
/// </summary>
/// <param name="Subject">The column that identifies the pupil or learner, so their names and DOB are replaced the same way in every file; if any.</param>
/// <param name="Sex">The column with the pupil's sex, to choose their fake forenames; if any.</param>
public sealed record FolderClassification(
    string? Subject,
    string? Sex,
    IReadOnlyDictionary<string, ColumnTransform> Columns)
{
    /// <returns><see langword="null"/> if the column isn't classified.</returns>
    public ColumnTransform? GetTransform(string column) =>
        Columns.TryGetValue(column, out ColumnTransform transform) ? transform : null;
}

/// <summary>
/// The checked-in classification of every column of every data file that's anonymised. A data file with a column that
/// isn't classified isn't anonymised, so personal data in a column nobody's looked at can't reach test.
/// </summary>
public sealed class AnonymisationClassification
{
    public const string DefaultFileName = "anonymisation.json";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower, allowIntegerValues: false) }
    };

    private readonly IReadOnlyDictionary<string, FolderClassification> _folders;

    private AnonymisationClassification(IReadOnlyList<PmrFormat> pmrFormats, IReadOnlyDictionary<string, FolderClassification> folders)
    {
        PmrFormats = pmrFormats;
        _folders = folders;
    }

    public IReadOnlyList<PmrFormat> PmrFormats { get; }

    /// <returns><see langword="null"/> if none of the folder's data files are classified.</returns>
    public FolderClassification? GetFolder(IngestFolder folder) => _folders.GetValueOrDefault(folder.Name);

    public static AnonymisationClassification Load(string path) => Parse(File.ReadAllText(path));

    public static AnonymisationClassification LoadShipped() => Load(Path.Combine(AppContext.BaseDirectory, DefaultFileName));

    public static AnonymisationClassification Parse(string json)
    {
        ClassificationJson file = JsonSerializer.Deserialize<ClassificationJson>(json, _jsonOptions)
            ?? throw new FormatException("The anonymisation classification is empty.");

        List<PmrFormat> pmrFormats = [.. (file.Identifiers?.Pmr ?? []).Select(f => new PmrFormat(
            IdentifierFormat.Parse(f.Source ?? throw new FormatException("A PMR format has no source.")),
            IdentifierFormat.Parse(f.Target ?? throw new FormatException("A PMR format has no target."))))];
        ValidatePmrFormats(pmrFormats);

        Dictionary<string, FolderClassification> folders = [];
        foreach ((string folderName, FolderJson folder) in file.Folders ?? [])
        {
            if (!IngestFolder.All.Any(f => f.Name == folderName))
            {
                throw new FormatException($"'{folderName}' isn't an ingest folder.");
            }

            Dictionary<string, ColumnTransform> columns = new(StringComparer.OrdinalIgnoreCase);
            foreach ((string column, ColumnTransform transform) in (folder.Transform ?? []).Select(c => (c.Key, c.Value))
                .Concat((folder.Keep ?? []).Select(c => (c, ColumnTransform.Keep))))
            {
                if (!columns.TryAdd(column, transform))
                {
                    throw new FormatException($"'{column}' is classified more than once in {folderName}.");
                }
            }

            if (folder.Subject is not null
                && columns.GetValueOrDefault(folder.Subject, ColumnTransform.Keep) is not (ColumnTransform.Pmr or ColumnTransform.Uln))
            {
                throw new FormatException($"The subject column of {folderName}, '{folder.Subject}', must be a PMR or ULN column.");
            }

            if (folder.Sex is not null && (!columns.TryGetValue(folder.Sex, out ColumnTransform sexTransform) || sexTransform != ColumnTransform.Keep))
            {
                throw new FormatException($"The sex column of {folderName}, '{folder.Sex}', must be kept.");
            }

            if (pmrFormats.Count == 0 && columns.ContainsValue(ColumnTransform.Pmr))
            {
                throw new FormatException($"{folderName} has a PMR column, but there are no PMR formats.");
            }

            folders[folderName] = new FolderClassification(folder.Subject, folder.Sex, columns);
        }

        return new AnonymisationClassification(pmrFormats, folders);
    }

    // Each real PMR must have a fake one of its own, and no fake PMR can be a real one
    private static void ValidatePmrFormats(List<PmrFormat> formats)
    {
        foreach (PmrFormat format in formats)
        {
            if (format.Target.Size < format.Source.Size)
            {
                throw new FormatException(
                    $"The PMR range {format.Target.Pattern} is smaller than {format.Source.Pattern}, so two real PMRs could get the same fake one.");
            }

            foreach (PmrFormat other in formats)
            {
                if (!format.Target.IsDisjointFrom(other.Source))
                {
                    throw new FormatException($"The PMR range {format.Target.Pattern} overlaps the real PMRs {other.Source.Pattern}.");
                }

                if (other != format && !format.Target.IsDisjointFrom(other.Target))
                {
                    throw new FormatException($"The PMR ranges {format.Target.Pattern} and {other.Target.Pattern} overlap.");
                }

                if (other != format && !format.Source.IsDisjointFrom(other.Source))
                {
                    throw new FormatException($"The PMR formats {format.Source.Pattern} and {other.Source.Pattern} overlap.");
                }
            }
        }
    }

    private sealed record ClassificationJson(IdentifiersJson? Identifiers, Dictionary<string, FolderJson>? Folders);

    private sealed record IdentifiersJson(List<PmrFormatJson>? Pmr);

    private sealed record PmrFormatJson(string? Source, string? Target);

    private sealed record FolderJson(string? Subject, string? Sex, Dictionary<string, ColumnTransform>? Transform, List<string>? Keep);
}
