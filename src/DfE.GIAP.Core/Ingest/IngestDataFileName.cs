using System.Globalization;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

namespace DfE.GIAP.Core.Ingest;

/// <param name="AcademicYear">E.g. <c>2024/2025</c>.</param>
public sealed record IngestDataFileAttributes(IngestDataset Dataset, string AcademicYear, IngestVersion IngestVersion);

/// <summary>
/// Reads the attributes of a data file from its name, e.g. <c>NPD_KS2_2025_U_KtS.txt</c> or
/// <c>NPD_Census_2425_KtS_Autumn.txt</c>.
/// </summary>
/// <remarks>
/// Ported from the old pipeline's <c>parse_file_characteristics_from_name</c>. The name is split into words on
/// <c>.</c>, <c>_</c>, <c>-</c>, <c>,</c> and spaces, and the words can be in any order.
/// </remarks>
public static class IngestDataFileName
{
    private static readonly char[] _separators = ['.', '_', '-', ',', ' '];

    private static readonly string[] _dataFileExtensions = [".txt", ".csv", ".tsv"];

    private static readonly string[] _metadataDocumentExtensions = [".xlsx", ".doc", ".docx"];

    private static readonly Dictionary<string, IngestDataset> _censusTerms = new()
    {
        ["autumn"] = IngestDataset.CensusAutumn,
        ["spring"] = IngestDataset.CensusSpring,
        ["summer"] = IngestDataset.CensusSummer
    };

    private static readonly Dictionary<string, IngestDataset> _furtherEducationTypes = new()
    {
        ["pp"] = IngestDataset.FurtherEducationPupilPremium,
        ["sen"] = IngestDataset.FurtherEducationSpecialEducationalNeeds
    };

    private static readonly Dictionary<string, IngestVersion> _maturities = new()
    {
        ["u"] = IngestVersion.Unamended,
        ["a"] = IngestVersion.Amended,
        ["f"] = IngestVersion.Final,
        ["final"] = IngestVersion.Final
    };

    // Words that identify a dataset; census and FE also need a word from _censusTerms or _furtherEducationTypes
    private static readonly string[] _datasetWords = ["census", "eyfsp", "ks1", "ks2", "ks4", "mtc", "phonics", "fe"];

    /// <summary>
    /// Metadata documents are shipped in zips alongside the data files; they aren't ingested.
    /// </summary>
    public static bool IsMetadataDocument(string fileName) =>
        _metadataDocumentExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);

    /// <exception cref="IngestDataFileNameException">The name isn't a valid data file name.</exception>
    public static IngestDataFileAttributes Parse(string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        if (!_dataFileExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase))
        {
            throw new IngestDataFileNameException(fileName, $"A data file must be one of {string.Join(", ", _dataFileExtensions)}.");
        }

        string lowerCaseName = fileName.ToLowerInvariant();
        HashSet<string> words = [.. lowerCaseName.Split(_separators, StringSplitOptions.RemoveEmptyEntries)];

        IngestDataset dataset = ParseDataset(fileName, lowerCaseName, words);
        string academicYear = ParseAcademicYear(fileName, dataset, words);
        IngestVersion ingestVersion = ParseIngestVersion(fileName, words);

        return new IngestDataFileAttributes(dataset, academicYear, ingestVersion);
    }

    private static IngestDataset ParseDataset(string fileName, string lowerCaseName, HashSet<string> words)
    {
        List<string> datasetWords = [.. _datasetWords.Where(words.Contains)];

        // "pupil-premium" is split into two words, so look for it in the whole name
        if (lowerCaseName.Contains("pupil-premium", StringComparison.Ordinal))
        {
            datasetWords.Add("pupil-premium");
        }

        if (datasetWords.Count != 1)
        {
            throw new IngestDataFileNameException(fileName, "The name must contain exactly one dataset.");
        }

        return datasetWords[0] switch
        {
            "census" => ParseQualifier(fileName, words, _censusTerms, "census term"),
            "fe" => ParseQualifier(fileName, words, _furtherEducationTypes, "further education type"),
            "eyfsp" => IngestDataset.Eyfsp,
            "ks1" => IngestDataset.Ks1,
            "ks2" => IngestDataset.Ks2,
            "ks4" => IngestDataset.Ks4,
            "mtc" => IngestDataset.Mtc,
            "phonics" => IngestDataset.Phonics,
            "pupil-premium" => IngestDataset.PupilPremium,
            _ => throw new InvalidOperationException()
        };
    }

    private static IngestDataset ParseQualifier(
        string fileName,
        HashSet<string> words,
        Dictionary<string, IngestDataset> qualifiers,
        string description)
    {
        IngestDataset[] matches = [.. qualifiers.Where(q => words.Contains(q.Key)).Select(q => q.Value)];

        return matches.Length == 1
            ? matches[0]
            : throw new IngestDataFileNameException(fileName, $"The name must contain exactly one {description}.");
    }

    // A census or pupil premium file's year is the academic year, e.g. 2425 for 2024/2025; for every other dataset
    // it's the calendar year the academic year ends in, e.g. 2025 for 2024/2025
    private static string ParseAcademicYear(string fileName, IngestDataset dataset, HashSet<string> words)
    {
        string[] numbers = [.. words.Where(w => w.All(char.IsAsciiDigit))];

        if (numbers.Length != 1 || numbers[0].Length != 4)
        {
            throw new IngestDataFileNameException(fileName, "The name must contain exactly one number, a 4-digit year.");
        }

        int year = int.Parse(numbers[0], CultureInfo.InvariantCulture);

        if (dataset is IngestDataset.CensusAutumn or IngestDataset.CensusSpring or IngestDataset.CensusSummer or IngestDataset.PupilPremium)
        {
            int startYear = year / 100;
            int endYear = year % 100;

            if (endYear != (startYear + 1) % 100)
            {
                throw new IngestDataFileNameException(fileName, $"A {(dataset is IngestDataset.PupilPremium ? "pupil premium" : "census")} file's year must be two consecutive 2-digit years, e.g. 2425.");
            }

            return $"20{startYear:00}/20{endYear:00}";
        }

        return $"{year - 1}/{year}";
    }

    // The old pipeline treats a name with no maturity as unamended
    private static IngestVersion ParseIngestVersion(string fileName, HashSet<string> words)
    {
        IngestVersion[] versions = [.. _maturities.Where(m => words.Contains(m.Key)).Select(m => m.Value).Distinct()];

        return versions.Length switch
        {
            0 => IngestVersion.Unamended,
            1 => versions[0],
            _ => throw new IngestDataFileNameException(fileName, "The name must contain at most one maturity (U, A or F).")
        };
    }
}

public sealed class IngestDataFileNameException(string fileName, string reason)
    : Exception($"'{fileName}' isn't a valid data file name. {reason}")
{
    public string FileName { get; } = fileName;
}
