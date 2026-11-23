using System.Text.Json.Nodes;

namespace DfE.GIAP.Verification.Comparison;

/// <summary>
/// A record as both pipelines can be compared in: a pupil, a learner or a data file's results.
/// </summary>
/// <param name="Fields">The record's own fields, e.g. a pupil's identifying fields, named as the old pipeline names them.</param>
/// <param name="Datasets">The rows of each dataset, e.g. <c>KS2</c>, named as the old pipeline names them.</param>
public sealed record ComparedRecord(
    string Key,
    IReadOnlyDictionary<string, JsonNode?> Fields,
    IReadOnlyDictionary<string, IReadOnlyList<JsonObject>> Datasets)
{
    public ComparedRecord(string key, IReadOnlyDictionary<string, JsonNode?> fields)
        : this(key, fields, new Dictionary<string, IReadOnlyList<JsonObject>>())
    {
    }
}

/// <summary>
/// Compares a record from each pipeline: every field, and every value of every row of every dataset.
/// </summary>
public static class RecordComparer
{
    public static IEnumerable<Difference> Compare(string comparison, string key, ComparedRecord? newRecord, ComparedRecord? oldRecord)
    {
        if (newRecord is null && oldRecord is null)
        {
            yield break;
        }

        if (newRecord is null)
        {
            yield return new Difference(comparison, DifferenceKinds.MissingInNew, Dataset: null, Field: null, key);
            yield break;
        }

        if (oldRecord is null)
        {
            yield return new Difference(comparison, DifferenceKinds.MissingInOld, Dataset: null, Field: null, key);
            yield break;
        }

        foreach (Difference difference in CompareFields(comparison, key, dataset: null, newRecord.Fields, oldRecord.Fields))
        {
            yield return difference;
        }

        foreach (string dataset in newRecord.Datasets.Keys.Union(oldRecord.Datasets.Keys).Order(StringComparer.Ordinal))
        {
            foreach (Difference difference in CompareRows(
                comparison,
                key,
                dataset,
                newRecord.Datasets.GetValueOrDefault(dataset) ?? [],
                oldRecord.Datasets.GetValueOrDefault(dataset) ?? []))
            {
                yield return difference;
            }
        }
    }

    /// <summary>
    /// Compares two lists of rows as multisets; see <see cref="RowMatcher"/>.
    /// </summary>
    public static IEnumerable<Difference> CompareRows(
        string comparison,
        string key,
        string? dataset,
        IReadOnlyList<JsonObject> newRows,
        IReadOnlyList<JsonObject> oldRows,
        string? identityColumn = null)
    {
        RowMatching matching = RowMatcher.Match(newRows, oldRows, identityColumn);

        foreach ((int newIndex, int oldIndex) in matching.Pairs)
        {
            foreach (Difference difference in CompareFields(comparison, key, dataset, newRows[newIndex], oldRows[oldIndex]))
            {
                yield return difference;
            }
        }

        foreach (int newIndex in matching.UnmatchedNew)
        {
            yield return new Difference(comparison, DifferenceKinds.RowMissingInOld, dataset, Field: null, key, NewValue: newRows[newIndex].ToJsonString());
        }

        foreach (int oldIndex in matching.UnmatchedOld)
        {
            yield return new Difference(comparison, DifferenceKinds.RowMissingInNew, dataset, Field: null, key, OldValue: oldRows[oldIndex].ToJsonString());
        }

        if (!matching.InOrder)
        {
            yield return new Difference(comparison, DifferenceKinds.RowOrder, dataset, Field: null, key);
        }
    }

    private static IEnumerable<Difference> CompareFields(
        string comparison,
        string key,
        string? dataset,
        IEnumerable<KeyValuePair<string, JsonNode?>> newFields,
        IEnumerable<KeyValuePair<string, JsonNode?>> oldFields)
    {
        Dictionary<string, JsonNode?> newValues = newFields.ToDictionary(f => f.Key, f => f.Value);
        Dictionary<string, JsonNode?> oldValues = oldFields.ToDictionary(f => f.Key, f => f.Value);

        foreach (string field in newValues.Keys.Union(oldValues.Keys).Order(StringComparer.Ordinal))
        {
            JsonNode? newValue = newValues.GetValueOrDefault(field);
            JsonNode? oldValue = oldValues.GetValueOrDefault(field);

            string? kind = ValueComparer.Compare(newValue, oldValue);
            if (kind is null)
            {
                continue;
            }

            // A field that's only on one side is reported as missing, rather than as a different value
            if (!newValues.ContainsKey(field))
            {
                kind = DifferenceKinds.FieldMissingInNew;
            }
            else if (!oldValues.ContainsKey(field))
            {
                kind = DifferenceKinds.FieldMissingInOld;
            }

            yield return new Difference(comparison, kind, dataset, field, key, newValue?.ToJsonString(), oldValue?.ToJsonString());
        }
    }
}
