using System.Text.Json.Nodes;

namespace DfE.GIAP.Verification.Comparison;

/// <param name="Pairs">The rows that were paired, as indexes into the new and old rows, in the new rows' order.</param>
/// <param name="UnmatchedNew">The new rows with no old row, as indexes.</param>
/// <param name="UnmatchedOld">The old rows with no new row, as indexes.</param>
public sealed record RowMatching(
    IReadOnlyList<(int New, int Old)> Pairs,
    IReadOnlyList<int> UnmatchedNew,
    IReadOnlyList<int> UnmatchedOld)
{
    /// <summary>
    /// Whether the paired rows are in the same order on both sides.
    /// </summary>
    public bool InOrder
    {
        get
        {
            for (int i = 1; i < Pairs.Count; i++)
            {
                if (Pairs[i].Old < Pairs[i - 1].Old)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

/// <summary>
/// Pairs up two lists of rows, compared as multisets.
/// </summary>
/// <remarks>
/// Rows whose values are all equal or equivalent are paired first. The rows left over are paired with the row on the
/// other side they have the most equal or equivalent values with, so a row with one wrong value is reported as that
/// value rather than as a missing row on each side. If an identity column is given, leftover rows are only paired
/// with rows with the same value in it.
/// </remarks>
public static class RowMatcher
{
    // Pairing leftover rows compares every leftover row with every other, so it's skipped for very different lists
    private const int MaxLeftoverRows = 1000;

    public static RowMatching Match(IReadOnlyList<JsonObject> newRows, IReadOnlyList<JsonObject> oldRows, string? identityColumn = null)
    {
        ArgumentNullException.ThrowIfNull(newRows);
        ArgumentNullException.ThrowIfNull(oldRows);

        List<(int New, int Old)> pairs = [];
        List<int> leftoverNew = [];

        Dictionary<string, Queue<int>> oldRowsByCanonicalForm = [];
        for (int i = 0; i < oldRows.Count; i++)
        {
            string canonicalForm = GetCanonicalForm(oldRows[i]);
            if (!oldRowsByCanonicalForm.TryGetValue(canonicalForm, out Queue<int>? indexes))
            {
                oldRowsByCanonicalForm[canonicalForm] = indexes = new Queue<int>();
            }

            indexes.Enqueue(i);
        }

        for (int i = 0; i < newRows.Count; i++)
        {
            if (oldRowsByCanonicalForm.TryGetValue(GetCanonicalForm(newRows[i]), out Queue<int>? indexes) && indexes.Count > 0)
            {
                pairs.Add((i, indexes.Dequeue()));
            }
            else
            {
                leftoverNew.Add(i);
            }
        }

        List<int> leftoverOld = [.. oldRowsByCanonicalForm.Values.SelectMany(q => q).Order()];

        if (leftoverNew.Count > 0 && leftoverOld.Count > 0 && (identityColumn is not null || (leftoverNew.Count <= MaxLeftoverRows && leftoverOld.Count <= MaxLeftoverRows)))
        {
            List<(int New, int Old)> leftoverPairs = PairLeftovers(newRows, oldRows, identityColumn, leftoverNew, leftoverOld);
            HashSet<int> pairedNew = [.. leftoverPairs.Select(p => p.New)];
            HashSet<int> pairedOld = [.. leftoverPairs.Select(p => p.Old)];

            pairs.AddRange(leftoverPairs);
            leftoverNew = [.. leftoverNew.Where(i => !pairedNew.Contains(i))];
            leftoverOld = [.. leftoverOld.Where(i => !pairedOld.Contains(i))];
        }

        return new RowMatching([.. pairs.OrderBy(p => p.New)], leftoverNew, leftoverOld);
    }

    private static List<(int New, int Old)> PairLeftovers(
        IReadOnlyList<JsonObject> newRows,
        IReadOnlyList<JsonObject> oldRows,
        string? identityColumn,
        List<int> leftoverNew,
        List<int> leftoverOld)
    {
        Dictionary<string, List<int>> oldByIdentity = leftoverOld
            .GroupBy(i => GetIdentity(oldRows[i], identityColumn))
            .ToDictionary(g => g.Key, g => g.ToList());

        List<(int New, int Old)> pairs = [];

        foreach (int newIndex in leftoverNew)
        {
            if (!oldByIdentity.TryGetValue(GetIdentity(newRows[newIndex], identityColumn), out List<int>? candidates)
                || candidates.Count == 0
                || candidates.Count > MaxLeftoverRows)
            {
                continue;
            }

            int bestOld = candidates.MaxBy(oldIndex => CountMatchingValues(newRows[newIndex], oldRows[oldIndex]));
            candidates.Remove(bestOld);
            pairs.Add((newIndex, bestOld));
        }

        return pairs;
    }

    private static string GetIdentity(JsonObject row, string? identityColumn) =>
        identityColumn is null ? string.Empty : ValueComparer.Canonicalise(row[identityColumn]) ?? string.Empty;

    private static int CountMatchingValues(JsonObject newRow, JsonObject oldRow) =>
        newRow.Count(p => ValueComparer.Canonicalise(p.Value) == ValueComparer.Canonicalise(oldRow[p.Key]));

    private static string GetCanonicalForm(JsonObject row) =>
        string.Join(
            '\u001f',
            row
                .Select(p => (p.Key, Value: ValueComparer.Canonicalise(p.Value)))
                .Where(p => p.Value is not null)
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => $"{p.Key}\u001e{p.Value}"));
}
