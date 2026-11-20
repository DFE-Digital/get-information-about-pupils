using System.Text;
using System.Text.Json.Nodes;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Core.PupilData;

namespace DfE.GIAP.Core.UnitTests.PupilData;

public class FurtherEducationDataFileReaderTests
{
    private const string AcademicYear = "2025/2026";
    private const string Header = "ULN\tSurname\tForename\tSex\tDOB\tACAD_YEAR";

    [Fact]
    public async Task ReadRowsAsync_MapsTheColumns()
    {
        // The columns are in a different order to the old pipeline's, and lower case
        (FurtherEducationFileRow[] rows, _) = await ReadAsync(
            "acad_year\tdob\tsex\tforename\tsurname\tuln\tNCYear\n" +
            "2025/26\t01/02/2005\tM\tAlex\tSmith\t1234567890\t12");

        FurtherEducationFileRow row = Assert.Single(rows);
        Assert.Equal(
            new FurtherEducationFileRow(
                1,
                "1234567890",
                Surname: "Smith",
                Forename: "Alex",
                Sex: "M",
                Dob: new DateOnly(2005, 2, 1),
                Data: row.Data),
            row);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(
                """
                {"acad_year": "2025/26", "dob": "2005-02-01T00:00:00.000Z", "sex": "M", "forename": "Alex", "surname": "Smith", "uln": "1234567890", "NCYear": "12"}
                """),
            JsonNode.Parse(row.Data)));
    }

    [Fact]
    public async Task OpenAsync_MissingColumn_Throws()
    {
        // The old pipeline's FE pupil premium columns had Sex misspelt as Sec
        IngestDataFileException exception = await Assert.ThrowsAsync<IngestDataFileException>(
            () => ReadAsync("ULN\tSurname\tForename\tSec\tDOB\n"));

        Assert.Equal("The data file is missing the following columns: 'Sex', 'ACAD_YEAR'.", exception.Message);
    }

    [Fact]
    public async Task OpenAsync_DuplicateColumn_Throws()
    {
        IngestDataFileException exception = await Assert.ThrowsAsync<IngestDataFileException>(() => ReadAsync(Header + "\tuln\n"));

        Assert.Equal("The data file has more than one of the following columns: 'ULN'.", exception.Message);
    }

    [Fact]
    public async Task OpenAsync_EmptyFile_Throws()
    {
        IngestDataFileException exception = await Assert.ThrowsAsync<IngestDataFileException>(() => ReadAsync(""));

        Assert.Equal("The data file is empty.", exception.Message);
    }

    [Fact]
    public async Task ReadRowsAsync_NoUln_RejectsRowWithEveryReason()
    {
        (FurtherEducationFileRow[] rows, FurtherEducationDataFileReader reader) = await ReadAsync(
            Header + "\n" + Row(" ", dob: "31/02/2005", academicYear: "2024/25") + "\n" + Row("2"));

        Assert.Equal(["2"], rows.Select(r => r.Uln));
        Assert.Equal(2, reader.RowsRead);
        IngestRowRejection rejection = Assert.Single(reader.RejectedRows);
        Assert.Equal(1, rejection.RowNumber);
        Assert.Equal(
            [
                "ULN is missing.",
                "DOB isn't a valid date: '31/02/2005'.",
                "ACAD_YEAR '2024/25' doesn't match the file's academic year, 2025/2026."
            ],
            rejection.Reasons);
        Assert.Equal("2024/25", rejection.Row["ACAD_YEAR"]);
    }

    [Theory]
    [InlineData("2025/26")]
    [InlineData("2025/2026")]
    public async Task ReadRowsAsync_FilesAcademicYear_IsWrittenAsItIsInData(string academicYear)
    {
        (FurtherEducationFileRow[] rows, _) = await ReadAsync(Header + "\n" + Row(academicYear: academicYear));

        Assert.Equal(academicYear, JsonNode.Parse(Assert.Single(rows).Data)!["ACAD_YEAR"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("2025/27")]
    [InlineData("2026/27")]
    [InlineData("25/26")]
    [InlineData("2526")]
    public async Task ReadRowsAsync_OtherAcademicYear_RejectsRow(string academicYear)
    {
        (FurtherEducationFileRow[] rows, FurtherEducationDataFileReader reader) = await ReadAsync(
            Header + "\n" + Row(academicYear: academicYear));

        Assert.Empty(rows);
        Assert.Equal(
            [$"ACAD_YEAR '{academicYear}' doesn't match the file's academic year, 2025/2026."],
            Assert.Single(reader.RejectedRows).Reasons);
    }

    [Fact]
    public async Task ReadRowsAsync_EmptyAcademicYear_IsWrittenWithNullInData()
    {
        (FurtherEducationFileRow[] rows, _) = await ReadAsync(Header + "\n" + Row(academicYear: ""));

        Assert.Null(JsonNode.Parse(Assert.Single(rows).Data)!["ACAD_YEAR"]);
    }

    [Fact]
    public async Task ReadRowsAsync_EmptyDob_IsNull()
    {
        (FurtherEducationFileRow[] rows, _) = await ReadAsync(Header + "\n" + Row(dob: ""));

        FurtherEducationFileRow row = Assert.Single(rows);
        Assert.Null(row.Dob);
        Assert.Null(JsonNode.Parse(row.Data)!["DOB"]);
    }

    [Fact]
    public async Task ReadRowsAsync_Values_AreKeptAsStrings()
    {
        (FurtherEducationFileRow[] rows, _) = await ReadAsync(Header + "\tSEN.Provision\tFTE\n" + Row() + "\tNULL\t0.5");

        JsonNode data = JsonNode.Parse(Assert.Single(rows).Data)!;
        Assert.Equal("NULL", data["SEN_Provision"]!.GetValue<string>());
        Assert.Equal("0.5", data["FTE"]!.GetValue<string>());
    }

    private static string Row(string uln = "1", string dob = "2005-02-01", string academicYear = "2025/26") =>
        $"{uln}\tSmith\tAlex\tM\t{dob}\t{academicYear}";

    private static async Task<(FurtherEducationFileRow[] Rows, FurtherEducationDataFileReader Reader)> ReadAsync(string content)
    {
        FurtherEducationDataFileReader reader = await FurtherEducationDataFileReader.OpenAsync(
            new MemoryStream(Encoding.UTF8.GetBytes(content)),
            AcademicYear);

        List<FurtherEducationFileRow> rows = [];
        await foreach (FurtherEducationFileRow row in reader.ReadRowsAsync())
        {
            rows.Add(row);
        }

        return ([.. rows], reader);
    }
}
