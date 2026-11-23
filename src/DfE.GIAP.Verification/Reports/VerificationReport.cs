using System.Globalization;
using System.Text;
using CsvHelper;
using DfE.GIAP.Verification.Comparison;

namespace DfE.GIAP.Verification.Reports;

/// <summary>
/// The differences of one kind, for one field of one dataset.
/// </summary>
/// <param name="AcceptedReason">Why the differences are accepted, or <see langword="null"/> if they aren't.</param>
/// <param name="ExampleKeys">Up to <see cref="VerificationReport.MaxExampleKeys"/> of the keys with the difference.</param>
public sealed record DifferenceGroup(
    string Comparison,
    string? Dataset,
    string? Field,
    string Kind,
    string? AcceptedReason,
    string? AcceptedDocs,
    long Count,
    IReadOnlyList<string> ExampleKeys);

/// <summary>
/// Collects the differences found by a comparison, grouped so that a systematic difference doesn't swamp the report.
/// </summary>
/// <remarks>
/// It's safe to add differences from more than one thread. The summary has no values, only counts and keys, as values
/// from production are personal data; values are only written to the detail CSV, if there is one.
/// </remarks>
public sealed class VerificationReport : IDisposable
{
    public const int MaxExampleKeys = 20;

    private readonly AcceptedDifferences _acceptedDifferences;
    private readonly CsvWriter? _detailWriter;
    private readonly Lock _lock = new();
    private readonly Dictionary<(string, string?, string?, string, AcceptedDifferenceRule?), Group> _groups = [];
    private readonly Dictionary<string, long> _comparedCounts = [];

    /// <param name="detail">Where to write every difference as CSV, if anywhere.</param>
    public VerificationReport(AcceptedDifferences acceptedDifferences, TextWriter? detail = null)
    {
        ArgumentNullException.ThrowIfNull(acceptedDifferences);

        _acceptedDifferences = acceptedDifferences;

        if (detail is not null)
        {
            _detailWriter = new CsvWriter(detail, CultureInfo.InvariantCulture);
            _detailWriter.WriteHeader<DetailRow>();
            _detailWriter.NextRecord();
        }
    }

    /// <summary>
    /// Whether there are no differences that haven't been accepted.
    /// </summary>
    public bool IsClean
    {
        get
        {
            lock (_lock)
            {
                return _groups.Keys.All(g => g.Item5 is not null);
            }
        }
    }

    public void Add(Difference difference)
    {
        ArgumentNullException.ThrowIfNull(difference);

        AcceptedDifferenceRule? rule = _acceptedDifferences.FindRule(difference);

        lock (_lock)
        {
            (string, string?, string?, string, AcceptedDifferenceRule?) key =
                (difference.Comparison, difference.Dataset, difference.Field, difference.Kind, rule);

            if (!_groups.TryGetValue(key, out Group? group))
            {
                _groups[key] = group = new Group();
            }

            group.Count++;
            if (group.ExampleKeys.Count < MaxExampleKeys && !group.ExampleKeys.Contains(difference.Key))
            {
                group.ExampleKeys.Add(difference.Key);
            }

            if (_detailWriter is not null)
            {
                _detailWriter.WriteRecord(new DetailRow(
                    difference.Comparison,
                    difference.Kind,
                    difference.Dataset,
                    difference.Field,
                    difference.Key,
                    difference.NewValue,
                    difference.OldValue,
                    rule?.Reason));
                _detailWriter.NextRecord();
            }
        }
    }

    /// <summary>
    /// Records how many records, files or rows were compared, so a clean report can't hide a comparison of nothing.
    /// </summary>
    public void AddCompared(string comparison, long count)
    {
        lock (_lock)
        {
            _comparedCounts[comparison] = _comparedCounts.GetValueOrDefault(comparison) + count;
        }
    }

    public IReadOnlyDictionary<string, long> ComparedCounts
    {
        get
        {
            lock (_lock)
            {
                return new SortedDictionary<string, long>(_comparedCounts, StringComparer.Ordinal);
            }
        }
    }

    /// <summary>
    /// The groups of differences, those that haven't been accepted first, then the largest first.
    /// </summary>
    public IReadOnlyList<DifferenceGroup> Groups
    {
        get
        {
            lock (_lock)
            {
                return
                [
                    .. _groups
                        .Select(g => new DifferenceGroup(
                            g.Key.Item1,
                            g.Key.Item2,
                            g.Key.Item3,
                            g.Key.Item4,
                            g.Key.Item5?.Reason,
                            g.Key.Item5?.Docs,
                            g.Value.Count,
                            [.. g.Value.ExampleKeys]))
                        .OrderBy(g => g.AcceptedReason is not null)
                        .ThenByDescending(g => g.Count)
                        .ThenBy(g => g.Comparison, StringComparer.Ordinal)
                        .ThenBy(g => g.Dataset, StringComparer.Ordinal)
                        .ThenBy(g => g.Field, StringComparer.Ordinal)
                        .ThenBy(g => g.Kind, StringComparer.Ordinal)
                ];
            }
        }
    }

    public string ToMarkdown(string title)
    {
        IReadOnlyList<DifferenceGroup> groups = Groups;
        DifferenceGroup[] unaccepted = [.. groups.Where(g => g.AcceptedReason is null)];
        DifferenceGroup[] accepted = [.. groups.Where(g => g.AcceptedReason is not null)];

        StringBuilder markdown = new();
        markdown.AppendLine(CultureInfo.InvariantCulture, $"# {title}");
        markdown.AppendLine();
        markdown.AppendLine(unaccepted.Length == 0
            ? "✅ No differences that haven't been accepted."
            : $"❌ {unaccepted.Sum(g => g.Count):N0} differences that haven't been accepted.");
        markdown.AppendLine();

        markdown.AppendLine("| Compared | Count |");
        markdown.AppendLine("| --- | ---: |");
        foreach ((string comparison, long count) in ComparedCounts)
        {
            markdown.AppendLine(CultureInfo.InvariantCulture, $"| {comparison} | {count:N0} |");
        }

        if (unaccepted.Length > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("## Differences");
            markdown.AppendLine();
            markdown.AppendLine("| Comparison | Dataset | Field | Kind | Count | Example keys |");
            markdown.AppendLine("| --- | --- | --- | --- | ---: | --- |");
            foreach (DifferenceGroup group in unaccepted)
            {
                markdown.AppendLine(CultureInfo.InvariantCulture,
                    $"| {group.Comparison} | {Cell(group.Dataset)} | {Cell(group.Field)} | {group.Kind} | {group.Count:N0} | {Cell(string.Join(", ", group.ExampleKeys))} |");
            }
        }

        if (accepted.Length > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("## Accepted differences");
            markdown.AppendLine();
            markdown.AppendLine("| Comparison | Dataset | Field | Kind | Count | Reason |");
            markdown.AppendLine("| --- | --- | --- | --- | ---: | --- |");
            foreach (DifferenceGroup group in accepted)
            {
                markdown.AppendLine(CultureInfo.InvariantCulture,
                    $"| {group.Comparison} | {Cell(group.Dataset)} | {Cell(group.Field)} | {group.Kind} | {group.Count:N0} | {Cell(group.AcceptedReason)} ([docs]({group.AcceptedDocs})) |");
            }
        }

        return markdown.ToString();
    }

    public void Dispose() => _detailWriter?.Dispose();

    private static string Cell(string? value) =>
        string.IsNullOrEmpty(value) ? "" : value.Replace("|", "\\|", StringComparison.Ordinal).ReplaceLineEndings(" ");

    private sealed class Group
    {
        public long Count { get; set; }
        public List<string> ExampleKeys { get; } = [];
    }

    private sealed record DetailRow(
        string Comparison,
        string Kind,
        string? Dataset,
        string? Field,
        string Key,
        string? NewValue,
        string? OldValue,
        string? AcceptedReason);
}
