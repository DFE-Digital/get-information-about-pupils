using System.IO.Compression;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Verification.Anonymisation;

/// <summary>
/// Describes a zip's data files without any of their values: their columns, how each is classified, and the shapes of
/// the identifiers in them. It's what the classification is written from.
/// </summary>
public static class ZipProfiler
{
    private const int MaxShapes = 20;

    private static readonly HashSet<ColumnTransform> _identifiers =
        [ColumnTransform.Pmr, ColumnTransform.Upn, ColumnTransform.Uln, ColumnTransform.CandidateNumber, ColumnTransform.Guid];

    /// <param name="zip">The real zip; it must be seekable.</param>
    public static void Profile(Stream zip, IngestFolder folder, AnonymisationClassification classification, TextWriter output)
    {
        FolderClassification? folderClassification = classification.GetFolder(folder);

        using ZipArchive archive = new(zip, ZipArchiveMode.Read, leaveOpen: true);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (!IngestFileProcessor.IsDataFile(entry))
            {
                output.WriteLine($"- `{entry.FullName}`: copied unchanged");
                continue;
            }

            output.WriteLine($"- `{entry.FullName}`, data file:");

            using Stream content = entry.Open();
            DataFileAnonymiser.RecordReader reader = new(content);
            DataFileAnonymiser.Record record = new();
            if (!reader.TryRead(record))
            {
                output.WriteLine("  - Empty");
                continue;
            }

            string[] header = [.. Enumerable.Range(0, record.Fields.Count).Select(i => DataFileAnonymiser.Decode(record, i, out _).Trim())];
            ColumnTransform?[] transforms = [.. header.Select(c => folderClassification?.GetTransform(c))];
            Dictionary<int, Dictionary<string, int>> shapes = Enumerable.Range(0, header.Length)
                .Where(i => transforms[i] is { } t && _identifiers.Contains(t))
                .ToDictionary(i => i, _ => new Dictionary<string, int>());
            Dictionary<int, Dictionary<char, int>> firstCharacters = shapes.Keys.ToDictionary(i => i, _ => new Dictionary<char, int>());

            int rows = 0;
            while (reader.TryRead(record))
            {
                rows++;
                foreach (int column in shapes.Keys.Where(c => c < record.Fields.Count))
                {
                    if (DataFileAnonymiser.ValueOrNull(DataFileAnonymiser.Decode(record, column, out _)) is { } value)
                    {
                        string shape = ValueAnonymiser.Shape(value);
                        shapes[column][shape] = shapes[column].GetValueOrDefault(shape) + 1;
                        firstCharacters[column][value[0]] = firstCharacters[column].GetValueOrDefault(value[0]) + 1;
                    }
                }
            }

            output.WriteLine($"  - {rows:N0} rows");
            for (int i = 0; i < header.Length; i++)
            {
                string classified = transforms[i] is { } transform ? transform.ToString() : "**not classified**";
                output.WriteLine($"  - `{header[i]}`: {classified}");

                if (shapes.TryGetValue(i, out Dictionary<string, int>? columnShapes))
                {
                    foreach ((string shape, int count) in columnShapes.OrderByDescending(s => s.Value).Take(MaxShapes))
                    {
                        output.WriteLine($"    - shape `{shape}`: {count:N0}");
                    }

                    if (columnShapes.Count > MaxShapes)
                    {
                        output.WriteLine($"    - and {columnShapes.Count - MaxShapes:N0} other shapes");
                    }

                    output.WriteLine($"    - first characters: {string.Join(", ", firstCharacters[i].OrderBy(c => c.Key).Select(c => $"`{c.Key}` {c.Value:N0}"))}");
                }
            }
        }
    }
}
