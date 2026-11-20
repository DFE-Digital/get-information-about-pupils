using System.Text;
using System.Text.Json.Nodes;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Core.PupilData;

namespace DfE.GIAP.Core.UnitTests.PupilData;

public class NpdDataFileReaderTests
{
    private const string AcademicYear = "2024/2025";

    private const string CensusHeader = "PupilMatchingRef\tAcademicYear\tLA\tEstab\tURN\tUPN\tSurname\tForename\tMiddleNames\tSex\tDOB";

    public static TheoryData<IngestDataset> Datasets => [.. NpdDatasetDefinition.All.Keys];

    [Theory]
    [MemberData(nameof(Datasets))]
    public async Task ReadRowsAsync_MapsTheDatasetsColumns(IngestDataset dataset)
    {
        NpdDatasetDefinition definition = NpdDatasetDefinition.All[dataset];
        // The columns are in a different order to the definition's, and lower case
        string[] columns = [.. definition.Columns.Reverse().Select(c => c.ToLowerInvariant()), "OTHER"];
        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase)
        {
            [definition.PupilMatchingRef] = "P1",
            [definition.LaCode] = "201",
            [definition.EstablishmentNumber] = "3614",
            [definition.Urn] = "100000",
            [definition.Upn] = "A123456789012",
            [definition.Surname] = "Smith",
            [definition.Forename] = "Alex",
            [definition.Sex] = "M",
            [definition.Dob] = "2015-02-01",
            [definition.AcademicYear] = AcademicYear,
            ["OTHER"] = "other"
        };
        if (definition.MiddleNames is not null)
        {
            values[definition.MiddleNames] = "Jo";
        }

        (NpdFileRow[] rows, _) = await ReadAsync(
            definition,
            string.Join('\t', columns) + "\n" + string.Join('\t', columns.Select(c => values[c])));

        NpdFileRow row = Assert.Single(rows);
        Assert.Equal(
            new NpdFileRow(
                1,
                "P1",
                Upn: "A123456789012",
                LaCode: 201,
                EstablishmentNumber: 3614,
                Urn: 100000,
                Surname: "Smith",
                Forename: "Alex",
                MiddleNames: definition.MiddleNames is null ? null : "Jo",
                Sex: "M",
                Dob: new DateOnly(2015, 2, 1),
                Data: row.Data),
            row);
        Assert.Equal("other", JsonNode.Parse(row.Data)!["OTHER"]!.GetValue<string>());
    }

    [Fact]
    public void Definitions_CoverEveryNpdDataset()
    {
        Assert.Equal(
            Enum.GetValues<NpdDataset>().Order(),
            NpdDatasetDefinition.All.Values.Select(d => d.Dataset).Order());
    }

    [Fact]
    public async Task OpenAsync_MissingColumn_Throws()
    {
        IngestDataFileException exception = await Assert.ThrowsAsync<IngestDataFileException>(
            () => ReadAsync(Census, "PupilMatchingRef\tAcademicYear\tLA\tEstab\tURN\tUPN\tSurname\tForename\tSex\n"));

        Assert.Equal("The data file is missing the following columns: 'MiddleNames', 'DOB'.", exception.Message);
    }

    [Fact]
    public async Task OpenAsync_DuplicateColumn_Throws()
    {
        IngestDataFileException exception = await Assert.ThrowsAsync<IngestDataFileException>(
            () => ReadAsync(Census, CensusHeader + "\tKS2.MAT\tks2_mat\n"));

        Assert.Equal("The data file has more than one of the following columns: 'KS2_MAT'.", exception.Message);
    }

    [Fact]
    public async Task OpenAsync_EmptyFile_Throws()
    {
        IngestDataFileException exception = await Assert.ThrowsAsync<IngestDataFileException>(() => ReadAsync(Census, ""));

        Assert.Equal("The data file is empty.", exception.Message);
    }

    [Theory]
    [InlineData("2015-02-01")]
    [InlineData("2015-02-01 00:00:00")]
    [InlineData("2015-02-01 00:00:00.000")]
    [InlineData("2015-02-01T00:00:00")]
    [InlineData("2015-02-01T00:00:00.000Z")]
    [InlineData("01/02/2015")]
    [InlineData("1/2/2015")]
    [InlineData("01/02/2015 00:00")]
    [InlineData("01/02/2015 00:00:00")]
    public async Task ReadRowsAsync_ValidDob_IsParsedDayFirstAndNormalised(string dob)
    {
        (NpdFileRow[] rows, _) = await ReadAsync(Census, CensusHeader + "\n" + CensusRow(dob: dob));

        NpdFileRow row = Assert.Single(rows);
        Assert.Equal(new DateOnly(2015, 2, 1), row.Dob);
        Assert.Equal("2015-02-01T00:00:00.000Z", JsonNode.Parse(row.Data)!["DOB"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("02/13/2015")]
    [InlineData("31/02/2015")]
    [InlineData("2015-13-01")]
    [InlineData("1 Feb 2015")]
    [InlineData("NULL")]
    [InlineData("00:00.0")]
    public async Task ReadRowsAsync_InvalidDob_RejectsRow(string dob)
    {
        (NpdFileRow[] rows, NpdDataFileReader reader) = await ReadAsync(Census, CensusHeader + "\n" + CensusRow(dob: dob));

        Assert.Empty(rows);
        IngestRowRejection rejection = Assert.Single(reader.RejectedRows);
        Assert.Equal(1, rejection.RowNumber);
        Assert.Equal([$"DOB isn't a valid date: '{dob}'."], rejection.Reasons);
        Assert.Equal(dob, rejection.Row["DOB"]);
    }

    [Fact]
    public async Task ReadRowsAsync_EmptyDob_IsNull()
    {
        (NpdFileRow[] rows, _) = await ReadAsync(Census, CensusHeader + "\n" + CensusRow(dob: ""));

        NpdFileRow row = Assert.Single(rows);
        Assert.Null(row.Dob);
        Assert.Null(JsonNode.Parse(row.Data)!["DOB"]);
    }

    [Theory]
    [InlineData("2019-09-01", "2019-09-01T00:00:00.000Z")]
    [InlineData("01/09/2019", "2019-09-01T00:00:00.000Z")]
    [InlineData("00:00.0", null)]
    [InlineData("", null)]
    public async Task ReadRowsAsync_EntryDate_IsNormalisedOrNull(string entryDate, string? expected)
    {
        (NpdFileRow[] rows, _) = await ReadAsync(Census, CensusHeader + "\tEntryDate\n" + CensusRow() + "\t" + entryDate);

        NpdFileRow row = Assert.Single(rows);
        Assert.Equal(expected, JsonNode.Parse(row.Data)!["EntryDate"]?.GetValue<string>());
    }

    [Fact]
    public async Task ReadRowsAsync_NoAcademicYear_IsWrittenButDataIsLeftAsItIs()
    {
        (NpdFileRow[] rows, _) = await ReadAsync(Census, CensusHeader + "\n" + CensusRow(academicYear: ""));

        NpdFileRow row = Assert.Single(rows);
        Assert.Null(JsonNode.Parse(row.Data)!["AcademicYear"]);
    }

    [Fact]
    public async Task ReadRowsAsync_AcademicYearNotTheFiles_RejectsRow()
    {
        (NpdFileRow[] rows, NpdDataFileReader reader) = await ReadAsync(Census, CensusHeader + "\n" + CensusRow(academicYear: "2023/2024"));

        Assert.Empty(rows);
        Assert.Equal(
            ["AcademicYear '2023/2024' doesn't match the file's academic year, 2024/2025."],
            Assert.Single(reader.RejectedRows).Reasons);
    }

    [Fact]
    public async Task ReadRowsAsync_NoPupilMatchingRef_RejectsRowWithEveryReason()
    {
        (NpdFileRow[] rows, NpdDataFileReader reader) = await ReadAsync(
            NpdDatasetDefinition.All[IngestDataset.Ks4],
            "KS4_PUPILMATCHINGREF\tKS4_LA\tKS4_ESTAB\tKS4_URN\tKS4_UPN\tKS4_SURNAME\tKS4_FORENAMES\tKS4_Sex\tKS4_DOB\tKS4_ACADYR\n" +
            " \t201\t3614\t100000\tA123456789012\tSmith\tAlex\tM\tbad\t2024/2025\n" +
            "P2\t201\t3614\t100000\tA123456789012\tSmith\tAlex\tM\t2015-02-01\t2024/2025\n");

        Assert.Equal(["P2"], rows.Select(r => r.PupilMatchingRef));
        Assert.Equal(2, reader.RowsRead);
        IngestRowRejection rejection = Assert.Single(reader.RejectedRows);
        Assert.Equal(1, rejection.RowNumber);
        Assert.Equal(["KS4_PUPILMATCHINGREF is missing.", "KS4_DOB isn't a valid date: 'bad'."], rejection.Reasons);
        Assert.Null(rejection.Row["KS4_PUPILMATCHINGREF"]);
    }

    [Fact]
    public async Task ReadRowsAsync_Values_AreTrimmedAndKeptAsStrings()
    {
        (NpdFileRow[] rows, _) = await ReadAsync(
            Census,
            CensusHeader + "\tKS2.MAT\tFSM\tSENType\tNote\n" + CensusRow(la: "NULL", surname: "  Smith ") + "\t0110\tNULL\t\t\"tab\there\"");

        NpdFileRow row = Assert.Single(rows);
        Assert.Null(row.LaCode);
        Assert.Equal("Smith", row.Surname);
        JsonNode data = JsonNode.Parse(row.Data)!;
        Assert.Equal("NULL", data["LA"]!.GetValue<string>());
        Assert.Equal("0110", data["KS2_MAT"]!.GetValue<string>());
        Assert.Equal("NULL", data["FSM"]!.GetValue<string>());
        Assert.Null(data["SENType"]);
        Assert.Equal("tab\there", data["Note"]!.GetValue<string>());
        Assert.Null(data["KS2.MAT"]);
    }

    [Fact]
    public async Task ReadRowsAsync_ShortRow_HasNullForMissingValues()
    {
        (NpdFileRow[] rows, _) = await ReadAsync(Census, CensusHeader + "\tExtra\n" + CensusRow());

        Assert.Null(JsonNode.Parse(Assert.Single(rows).Data)!["Extra"]);
    }

    [Fact]
    public async Task ReadRowsAsync_FileWithByteOrderMark_IsRead()
    {
        (NpdFileRow[] rows, _) = await ReadAsync(Census, "﻿" + CensusHeader + "\n" + CensusRow());

        Assert.Equal("P1", Assert.Single(rows).PupilMatchingRef);
    }

    [Fact]
    public async Task ReadRowsAsync_NumbersRowsInFileOrder()
    {
        (NpdFileRow[] rows, NpdDataFileReader reader) = await ReadAsync(
            Census,
            CensusHeader + "\n" + CensusRow("P1") + "\n" + CensusRow("P2") + "\n\n" + CensusRow("P1") + "\n");

        Assert.Equal([(1, "P1"), (2, "P2"), (3, "P1")], rows.Select(r => (r.RowNumber, r.PupilMatchingRef)));
        Assert.Equal(3, reader.RowsRead);
    }

    private static NpdDatasetDefinition Census => NpdDatasetDefinition.All[IngestDataset.CensusAutumn];

    private static string CensusRow(
        string pupilMatchingRef = "P1",
        string academicYear = AcademicYear,
        string la = "201",
        string surname = "Smith",
        string dob = "2015-02-01") =>
        $"{pupilMatchingRef}\t{academicYear}\t{la}\t3614\t100000\tA123456789012\t{surname}\tAlex\t\tM\t{dob}";

    private static async Task<(NpdFileRow[] Rows, NpdDataFileReader Reader)> ReadAsync(NpdDatasetDefinition definition, string content)
    {
        NpdDataFileReader reader = await NpdDataFileReader.OpenAsync(
            new MemoryStream(Encoding.UTF8.GetBytes(content)),
            definition,
            AcademicYear);

        List<NpdFileRow> rows = [];
        await foreach (NpdFileRow row in reader.ReadRowsAsync())
        {
            rows.Add(row);
        }

        return ([.. rows], reader);
    }
}
