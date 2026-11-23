using System.Text.Json.Nodes;
using DfE.GIAP.Verification.Comparison;

namespace DfE.GIAP.Verification.Tests.Comparison;

public class RecordComparerTests
{
    private const string Comparison = "pupil-noskill";
    private const string Key = "PMR1";

    [Fact]
    public void Compare_IdenticalRecords_HasNoDifferences()
    {
        Assert.Empty(RecordComparer.Compare(Comparison, Key, Pupil(), Pupil()));
    }

    [Fact]
    public void Compare_MissingRecord_ReportsWhichSideItsMissingFrom()
    {
        Assert.Equal(
            [new Difference(Comparison, DifferenceKinds.MissingInNew, null, null, Key)],
            RecordComparer.Compare(Comparison, Key, null, Pupil()));
        Assert.Equal(
            [new Difference(Comparison, DifferenceKinds.MissingInOld, null, null, Key)],
            RecordComparer.Compare(Comparison, Key, Pupil(), null));
    }

    [Fact]
    public void Compare_ForenameAndSurnameSwapped_ReportsBothFieldsAndEveryRowsValues()
    {
        ComparedRecord swapped = Pupil(forename: "Smith", surname: "Alex");

        Difference[] differences = [.. RecordComparer.Compare(Comparison, Key, swapped, Pupil())];

        Assert.Equal(
            [
                new Difference(Comparison, DifferenceKinds.ValueDifferent, null, "Forename", Key, "\"Smith\"", "\"Alex\""),
                new Difference(Comparison, DifferenceKinds.ValueDifferent, null, "Surname", Key, "\"Alex\"", "\"Smith\""),
                new Difference(Comparison, DifferenceKinds.ValueDifferent, "KS2", "FORENAMES", Key, "\"Smith\"", "\"Alex\""),
                new Difference(Comparison, DifferenceKinds.ValueDifferent, "KS2", "SURNAME", Key, "\"Alex\"", "\"Smith\"")
            ],
            differences);
    }

    [Fact]
    public void Compare_TypeInferredValues_ReportsThemAsEquivalent()
    {
        ComparedRecord oldPupil = Pupil(la: JsonValue.Create(201), dob: "2010-03-01T00:00:00.000Z");

        Difference[] differences = [.. RecordComparer.Compare(Comparison, Key, Pupil(), oldPupil)];

        Assert.Equal(
            [
                new Difference(Comparison, DifferenceKinds.DateEquivalent, null, "DOB", Key, "\"2010-03-01\"", "\"2010-03-01T00:00:00.000Z\""),
                new Difference(Comparison, DifferenceKinds.NumberEquivalent, null, "LA", Key, "\"201\"", "201")
            ],
            differences);
    }

    [Fact]
    public void Compare_FieldOnOneSide_ReportsItAsMissing()
    {
        ComparedRecord oldPupil = Pupil(extraField: ("ConcatenatedName", "Alex Smith"));

        Difference difference = Assert.Single(RecordComparer.Compare(Comparison, Key, Pupil(), oldPupil));

        Assert.Equal(
            new Difference(Comparison, DifferenceKinds.FieldMissingInNew, null, "ConcatenatedName", Key, null, "\"Alex Smith\""),
            difference);
    }

    [Fact]
    public void Compare_NullAndMissingField_AreEqual()
    {
        ComparedRecord oldPupil = Pupil(extraField: ("Middlenames", null));

        Assert.Empty(RecordComparer.Compare(Comparison, Key, Pupil(), oldPupil));
    }

    [Fact]
    public void Compare_RowsInADifferentOrder_ReportsRowOrderOnly()
    {
        JsonObject first = Row("2023/2024", "100");
        JsonObject second = Row("2024/2025", "101");

        Difference difference = Assert.Single(RecordComparer.Compare(
            Comparison,
            Key,
            Pupil(rows: [first, second]),
            Pupil(rows: [second.DeepClone().AsObject(), first.DeepClone().AsObject()])));

        Assert.Equal(new Difference(Comparison, DifferenceKinds.RowOrder, "KS2", null, Key), difference);
    }

    [Fact]
    public void Compare_ExtraRow_ReportsTheMissingRow()
    {
        JsonObject extra = Row("2024/2025", "101");

        Difference difference = Assert.Single(RecordComparer.Compare(
            Comparison,
            Key,
            Pupil(rows: [Row("2023/2024", "100")]),
            Pupil(rows: [Row("2023/2024", "100"), extra])));

        Assert.Equal(DifferenceKinds.RowMissingInNew, difference.Kind);
        Assert.Equal("KS2", difference.Dataset);
        Assert.True(JsonNode.DeepEquals(extra, JsonNode.Parse(difference.OldValue!)));
    }

    [Fact]
    public void Compare_RowWithOneWrongValue_IsPairedAndReportsThatValue()
    {
        Difference difference = Assert.Single(RecordComparer.Compare(
            Comparison,
            Key,
            Pupil(rows: [Row("2023/2024", "100"), Row("2024/2025", "101")]),
            Pupil(rows: [Row("2023/2024", "100"), Row("2024/2025", "999")])));

        Assert.Equal(
            new Difference(Comparison, DifferenceKinds.ValueDifferent, "KS2", "KS2_TOTMARK", Key, "\"101\"", "\"999\""),
            difference);
    }

    [Fact]
    public void CompareRows_WithIdentityColumn_OnlyPairsRowsWithTheSameIdentity()
    {
        Difference[] differences = [.. RecordComparer.CompareRows(
            "rejected-rows",
            "NPD_KS2_2025_U.txt",
            "KS2",
            [new JsonObject { ["PupilMatchingRef"] = "A", ["DOB"] = "x" }],
            [new JsonObject { ["PupilMatchingRef"] = "B", ["DOB"] = "x" }],
            identityColumn: "PupilMatchingRef")];

        Assert.Equal(
            [DifferenceKinds.RowMissingInOld, DifferenceKinds.RowMissingInNew],
            differences.Select(d => d.Kind));
    }

    private static ComparedRecord Pupil(
        string forename = "Alex",
        string surname = "Smith",
        JsonNode? la = null,
        string dob = "2010-03-01",
        (string Name, string? Value)? extraField = null,
        IReadOnlyList<JsonObject>? rows = null)
    {
        Dictionary<string, JsonNode?> fields = new()
        {
            ["PupilMatchingRef"] = Key,
            ["Forename"] = forename,
            ["Surname"] = surname,
            ["LA"] = la ?? "201",
            ["DOB"] = dob
        };

        if (extraField is { } field)
        {
            fields[field.Name] = field.Value;
        }

        rows ??= [Row("2024/2025", "101", forename, surname)];

        return new ComparedRecord(Key, fields, new Dictionary<string, IReadOnlyList<JsonObject>> { ["KS2"] = rows });
    }

    private static JsonObject Row(string academicYear, string totalMark, string forename = "Alex", string surname = "Smith") =>
        new()
        {
            ["PupilMatchingRef"] = Key,
            ["FORENAMES"] = forename,
            ["SURNAME"] = surname,
            ["ACADYR"] = academicYear,
            ["KS2_TOTMARK"] = totalMark
        };
}
