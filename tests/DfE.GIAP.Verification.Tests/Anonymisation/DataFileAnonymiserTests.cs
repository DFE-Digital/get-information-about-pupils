using System.Text;
using DfE.GIAP.Verification.Anonymisation;

namespace DfE.GIAP.Verification.Tests.Anonymisation;

public sealed class DataFileAnonymiserTests : IDisposable
{
    private static readonly FolderClassification _classification = new(
        "PupilMatchingRef",
        "GENDER",
        new Dictionary<string, ColumnTransform>(StringComparer.OrdinalIgnoreCase)
        {
            ["PupilMatchingRef"] = ColumnTransform.Pmr,
            ["UPN"] = ColumnTransform.Upn,
            ["SURNAME"] = ColumnTransform.Surname,
            ["FORENAMES"] = ColumnTransform.Forename,
            ["DOB"] = ColumnTransform.Dob,
            ["GENDER"] = ColumnTransform.Keep,
            ["LA"] = ColumnTransform.Keep,
            ["NOTE"] = ColumnTransform.Keep
        });

    private readonly ValueAnonymiser _values = new(ValueAnonymiserTests.Key, ValueAnonymiserTests.PmrFormats, ValueAnonymiserTests.Names);

    public void Dispose() => _values.Dispose();

    [Fact]
    public void Anonymise_ReplacesOnlyTheClassifiedValues_AndKeepsEveryOtherByte()
    {
        byte[] input = [
            0xEF, 0xBB, 0xBF,
            .. Encoding.UTF8.GetBytes(
                "PupilMatchingRef\tUPN\tSURNAME\tFORENAMES\tDOB\tGENDER\tLA\tNOTE\r\n" +
                "M891313215015\tP000000000001\t\"Gilby\"\t Skylar \t20/10/2008\tM\t937\t\"a\ttab, \"\"quote\"\" and\nline\"\r\n" +
                "B909250813237\tNULL\t\t\t\tF\t938\t\n")
        ];

        string output = Anonymise(input, out int rows);
        string[] lines = output.Split("\r\n");

        Assert.Equal(2, rows);
        Assert.StartsWith("﻿PupilMatchingRef\tUPN\tSURNAME\tFORENAMES\tDOB\tGENDER\tLA\tNOTE", output, StringComparison.Ordinal);
        Assert.Matches("^[A-Z]{2}[0-9]{11}\t[A-Z][0-9]{12}\t\"[A-Z][a-z]+\"\t [A-Z][a-z]+ \t(1[3-9]|2[0-9]|3[01])/10/2008\tM\t937\t\"a\ttab, \"\"quote\"\" and\nline\"$", lines[1]);
        Assert.Matches("^[A-Z]{2}[0-9]{11}\tNULL\t\t\t\tF\t938\t\n$", lines[2]);
        Assert.DoesNotContain("M891313215015", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Gilby", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Skylar", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Anonymise_ColumnsThatArentClassified_Throws_NamingThem()
    {
        byte[] input = Encoding.UTF8.GetBytes("PupilMatchingRef\tAddress1\n");

        AnonymisationException exception = Assert.Throws<AnonymisationException>(() => Anonymise(input, out _));

        Assert.Contains("'Address1'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Anonymise_ARowWithMoreValuesThanTheHeader_Throws()
    {
        byte[] input = Encoding.UTF8.GetBytes("PupilMatchingRef\tLA\nM891313215015\t937\tGilby\n");

        Assert.Throws<AnonymisationException>(() => Anonymise(input, out _));
    }

    [Fact]
    public void Anonymise_AReplacedValueThatsAlsoInAKeptColumn_Throws_WithoutTheValue()
    {
        byte[] input = Encoding.UTF8.GetBytes("PupilMatchingRef\tSURNAME\tNOTE\nM891313215015\tGilby\tgilby\n");

        AnonymisationException exception = Assert.Throws<AnonymisationException>(() => Anonymise(input, out _));

        Assert.Contains("'NOTE'", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("ilby", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Anonymise_AnEmptyFile_WritesNothing()
    {
        Assert.Equal(string.Empty, Anonymise([], out int rows));
        Assert.Equal(0, rows);
    }

    private string Anonymise(byte[] input, out int rows)
    {
        using MemoryStream output = new();
        rows = new DataFileAnonymiser(_values).Anonymise(new MemoryStream(input), output, _classification);
        return Encoding.UTF8.GetString(output.ToArray());
    }
}
