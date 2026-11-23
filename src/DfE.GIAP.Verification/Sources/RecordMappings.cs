using System.Globalization;
using System.Text.Json.Nodes;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Verification.Comparison;

namespace DfE.GIAP.Verification.Sources;

/// <summary>
/// A Cosmos container the old pipeline writes pupils or learners to, and what it's compared with in Postgres.
/// </summary>
/// <param name="Name">The container's key in <c>RepositoryOptions:Containers</c>, and the name of the comparison.</param>
/// <param name="KeyField">The document field that identifies a pupil or learner; it's also the partition key.</param>
/// <param name="Datasets">The document fields that hold a dataset's rows.</param>
public sealed record RecordContainer(string Name, string KeyField, IReadOnlyCollection<string> Datasets)
{
    public static RecordContainer NationalPupilDatabase { get; } = new(
        "pupil-noskill",
        "PupilMatchingRef",
        [.. RecordMappings.NpdDatasetNames.Values]);

    public static RecordContainer PupilPremium { get; } = new("pupil-premium", "PupilMatchingRef", [RecordMappings.PupilPremiumDatasetName]);

    public static RecordContainer FurtherEducation { get; } = new(
        "further-education",
        "ULN",
        [.. RecordMappings.FurtherEducationDatasetNames.Values]);

    public static IReadOnlyList<RecordContainer> All { get; } = [NationalPupilDatabase, PupilPremium, FurtherEducation];
}

/// <summary>
/// Maps what each pipeline wrote to a <see cref="ComparedRecord"/>, with the old pipeline's names.
/// </summary>
public static class RecordMappings
{
    public const string PupilPremiumDatasetName = "Pupil_Premium";

    /// <remarks>The old pipeline's <c>master_fields_from_type_mapping</c>.</remarks>
    public static IReadOnlyDictionary<NpdDataset, string> NpdDatasetNames { get; } = new Dictionary<NpdDataset, string>
    {
        [NpdDataset.CensusAutumn] = "Census_Autumn",
        [NpdDataset.CensusSpring] = "Census_Spring",
        [NpdDataset.CensusSummer] = "Census_Summer",
        [NpdDataset.Eyfsp] = "EYFSP",
        [NpdDataset.Phonics] = "Phonics",
        [NpdDataset.Ks1] = "KS1",
        [NpdDataset.Ks2] = "KS2",
        [NpdDataset.Ks4] = "KS4",
        [NpdDataset.Mtc] = "MTC"
    };

    public static IReadOnlyDictionary<FurtherEducationDataset, string> FurtherEducationDatasetNames { get; } =
        new Dictionary<FurtherEducationDataset, string>
        {
            [FurtherEducationDataset.PupilPremium] = PupilPremiumDatasetName,
            [FurtherEducationDataset.SpecialEducationalNeeds] = "SEN"
        };

    public static ComparedRecord FromNpd(NpdPupil pupil, IEnumerable<NpdPupilRecord> records) =>
        new(
            pupil.PupilMatchingRef,
            new Dictionary<string, JsonNode?>
            {
                ["PupilMatchingRef"] = pupil.PupilMatchingRef,
                ["UPN"] = pupil.Upn,
                ["LA"] = pupil.LaCode,
                ["Estab"] = pupil.EstablishmentNumber,
                ["URN"] = pupil.Urn,
                ["Surname"] = pupil.Surname,
                ["Forename"] = pupil.Forename,
                ["Middlenames"] = pupil.MiddleNames,
                ["Sex"] = pupil.Sex,
                ["DOB"] = FormatDate(pupil.Dob)
            },
            records
                .GroupBy(r => NpdDatasetNames[r.Dataset])
                .ToDictionary(
                    g => g.Key,
                    g => ParseRows(g.OrderBy(r => r.AcademicYear, StringComparer.Ordinal).Select(r => r.Data))));

    public static ComparedRecord FromPupilPremium(PupilPremiumPupil pupil) =>
        new(
            pupil.PupilMatchingRef,
            new Dictionary<string, JsonNode?>
            {
                ["PupilMatchingRef"] = pupil.PupilMatchingRef,
                ["UPN"] = pupil.Upn,
                ["LA"] = pupil.LaCode,
                ["Estab"] = pupil.EstablishmentNumber,
                ["URN"] = pupil.Urn,
                ["Surname"] = pupil.Surname,
                ["Forename"] = pupil.Forename,
                ["Middlenames"] = pupil.MiddleNames,
                ["Sex"] = pupil.Sex,
                ["DOB"] = FormatDate(pupil.Dob),
                ["Source_flag"] = pupil.SourceFlag
            },
            new Dictionary<string, IReadOnlyList<JsonObject>> { [PupilPremiumDatasetName] = ParseRows([pupil.Data]) });

    public static ComparedRecord FromFurtherEducation(FurtherEducationLearner learner, IEnumerable<FurtherEducationLearnerRecord> records) =>
        new(
            learner.Uln,
            new Dictionary<string, JsonNode?>
            {
                ["ULN"] = learner.Uln,
                ["Surname"] = learner.Surname,
                ["Forename"] = learner.Forename,
                ["Sex"] = learner.Sex,
                ["DOB"] = FormatDate(learner.Dob)
            },
            records
                .GroupBy(r => FurtherEducationDatasetNames[r.Dataset])
                .ToDictionary(
                    g => g.Key,
                    g => ParseRows(g.OrderBy(r => r.AcademicYear, StringComparer.Ordinal).Select(r => r.Data))));

    /// <summary>
    /// Maps a document from one of the old pipeline's containers. Every field other than Cosmos DB's own (<c>id</c>
    /// and those starting <c>_</c>) and the datasets' rows is one of the record's fields.
    /// </summary>
    public static ComparedRecord FromCosmosDocument(JsonObject document, RecordContainer container)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(container);

        string key = document[container.KeyField]?.ToString()
            ?? throw new ArgumentException($"The document has no {container.KeyField}.", nameof(document));

        Dictionary<string, JsonNode?> fields = [];
        Dictionary<string, IReadOnlyList<JsonObject>> datasets = [];

        foreach ((string name, JsonNode? value) in document)
        {
            if (name == "id" || name.StartsWith('_'))
            {
                continue;
            }

            if (container.Datasets.Contains(name) && value is JsonArray rows)
            {
                datasets[name] = [.. rows.OfType<JsonObject>().Select(r => r.DeepClone().AsObject())];
            }
            else
            {
                fields[name] = value?.DeepClone();
            }
        }

        return new ComparedRecord(key, fields, datasets);
    }

    private static string? FormatDate(DateOnly? date) => date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static IReadOnlyList<JsonObject> ParseRows(IEnumerable<string> data) =>
        [.. data.SelectMany(d => JsonNode.Parse(d)?.AsArray() ?? []).OfType<JsonObject>().Select(r => r.DeepClone().AsObject())];
}
