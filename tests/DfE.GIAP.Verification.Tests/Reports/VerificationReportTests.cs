using DfE.GIAP.Verification.Comparison;
using DfE.GIAP.Verification.Reports;

namespace DfE.GIAP.Verification.Tests.Reports;

public class VerificationReportTests
{
    private static readonly AcceptedDifferenceRule _rowOrderRule =
        new(DifferenceKinds.RowOrder, "downloads", null, null, null, null, "Spark wrote rows in any order", "docs/file-ingestion.md#prepared-downloads");

    [Fact]
    public void Add_GroupsDifferencesAndKeepsUpTo20ExampleKeys()
    {
        using VerificationReport report = new(new AcceptedDifferences([]));

        for (int i = 0; i < 25; i++)
        {
            report.Add(new Difference("pupil-noskill", DifferenceKinds.ValueDifferent, "KS2", "SURNAME", $"PMR{i}", "\"a\"", "\"b\""));
        }

        DifferenceGroup group = Assert.Single(report.Groups);
        Assert.Equal(25, group.Count);
        Assert.Equal(VerificationReport.MaxExampleKeys, group.ExampleKeys.Count);
        Assert.False(report.IsClean);
    }

    [Fact]
    public void IsClean_OnlyAcceptedDifferences_IsTrue()
    {
        using VerificationReport report = new(new AcceptedDifferences([_rowOrderRule]));

        report.Add(new Difference("downloads", DifferenceKinds.RowOrder, "LA", null, "LA/201/ks2_2025_cla.csv"));

        Assert.True(report.IsClean);
        Assert.Equal(_rowOrderRule.Reason, Assert.Single(report.Groups).AcceptedReason);
    }

    [Fact]
    public void ToMarkdown_HasNoValues()
    {
        using VerificationReport report = new(new AcceptedDifferences([]));
        report.Add(new Difference("pupil-noskill", DifferenceKinds.ValueDifferent, "KS2", "SURNAME", "PMR1", "\"Smith\"", "\"Jones\""));
        report.AddCompared("pupil-noskill", 1);

        string markdown = report.ToMarkdown("Verification");

        Assert.Contains("| pupil-noskill | KS2 | SURNAME | value-different | 1 | PMR1 |", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain("Smith", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain("Jones", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Add_WithDetail_WritesEveryDifferenceWithItsValues()
    {
        StringWriter detail = new();
        using (VerificationReport report = new(new AcceptedDifferences([_rowOrderRule]), detail))
        {
            report.Add(new Difference("pupil-noskill", DifferenceKinds.ValueDifferent, "KS2", "SURNAME", "PMR1", "\"Smith\"", "\"Jones\""));
            report.Add(new Difference("downloads", DifferenceKinds.RowOrder, "LA", null, "LA/201/ks2_2025_cla.csv"));
        }

        Assert.Equal(
            "Comparison,Kind,Dataset,Field,Key,NewValue,OldValue,AcceptedReason\n" +
            "pupil-noskill,value-different,KS2,SURNAME,PMR1,\"\"\"Smith\"\"\",\"\"\"Jones\"\"\",\n" +
            "downloads,row-order,LA,,LA/201/ks2_2025_cla.csv,,,Spark wrote rows in any order\n",
            detail.ToString().ReplaceLineEndings("\n"));
    }
}
