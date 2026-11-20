using System.Text;
using System.Text.Json.Nodes;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Core.PupilData;

namespace DfE.GIAP.Core.UnitTests.PupilData;

public class PupilPremiumDataFileReaderTests
{
    private const string Header = "PupilMatchingRef\tLA\tEstab\tURN\tUPN\tSurname\tForename\tMiddlenames\tSex\tDOB\tSource\tAdopted";

    [Fact]
    public async Task ReadRowsAsync_MapsTheColumns()
    {
        // The columns are in a different order to the old pipeline's, and lower case
        (PupilPremiumFileRow[] rows, _) = await ReadAsync(
            "source\tdob\tsex\tmiddlenames\tforename\tsurname\tupn\turn\testab\tla\tpupilmatchingref\tadopted\tNCYear\n" +
            "C\t01/02/2015\tM\tJo\tAlex\tSmith\tA123456789012\t100000\t3614\t201\tP1\t0\t5");

        PupilPremiumFileRow row = Assert.Single(rows);
        Assert.Equal(
            new PupilPremiumFileRow(
                1,
                "P1",
                Upn: "A123456789012",
                LaCode: 201,
                EstablishmentNumber: 3614,
                Urn: 100000,
                Surname: "Smith",
                Forename: "Alex",
                MiddleNames: "Jo",
                Sex: "M",
                Dob: new DateOnly(2015, 2, 1),
                Source: "C",
                Data: row.Data),
            row);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(
                """
                {"source": "C", "dob": "2015-02-01T00:00:00.000Z", "sex": "M", "middlenames": "Jo", "forename": "Alex", "surname": "Smith", "upn": "A123456789012", "urn": "100000", "estab": "3614", "la": "201", "pupilmatchingref": "P1", "AdoptedfromCarePremium": "0", "NCYear": "5"}
                """),
            JsonNode.Parse(row.Data)));
    }

    [Fact]
    public async Task OpenAsync_MissingColumn_Throws()
    {
        IngestDataFileException exception = await Assert.ThrowsAsync<IngestDataFileException>(
            () => ReadAsync("PupilMatchingRef\tLA\tEstab\tURN\tUPN\tSurname\tForename\tMiddlenames\tSex\tDOB\n"));

        Assert.Equal("The data file is missing the following columns: 'Source', 'Adopted'.", exception.Message);
    }

    [Fact]
    public async Task OpenAsync_DuplicateColumn_Throws()
    {
        IngestDataFileException exception = await Assert.ThrowsAsync<IngestDataFileException>(() => ReadAsync(Header + "\tsource\n"));

        Assert.Equal("The data file has more than one of the following columns: 'Source'.", exception.Message);
    }

    [Fact]
    public async Task OpenAsync_EmptyFile_Throws()
    {
        IngestDataFileException exception = await Assert.ThrowsAsync<IngestDataFileException>(() => ReadAsync(""));

        Assert.Equal("The data file is empty.", exception.Message);
    }

    [Fact]
    public async Task ReadRowsAsync_AdoptedPupil_RejectsRow()
    {
        (PupilPremiumFileRow[] rows, PupilPremiumDataFileReader reader) = await ReadAsync(
            Header + "\n" + Row("P1", adopted: "1") + "\n" + Row("P2", adopted: "0"));

        Assert.Equal(["P2"], rows.Select(r => r.PupilMatchingRef));
        IngestRowRejection rejection = Assert.Single(reader.RejectedRows);
        Assert.Equal(1, rejection.RowNumber);
        Assert.Equal(["Adopted is 1; pupils adopted from care aren't loaded."], rejection.Reasons);
        Assert.Equal("P1", rejection.Row["PupilMatchingRef"]);
    }

    [Fact]
    public async Task ReadRowsAsync_EmptyAdopted_IsWritten()
    {
        (PupilPremiumFileRow[] rows, _) = await ReadAsync(Header + "\n" + Row(adopted: ""));

        Assert.Null(JsonNode.Parse(Assert.Single(rows).Data)!["AdoptedfromCarePremium"]);
    }

    [Fact]
    public async Task ReadRowsAsync_NoPupilMatchingRef_RejectsRowWithEveryReason()
    {
        (PupilPremiumFileRow[] rows, PupilPremiumDataFileReader reader) = await ReadAsync(
            Header + "\n" + Row(" ", dob: "31/02/2015", adopted: "1") + "\n" + Row("P2"));

        Assert.Equal(["P2"], rows.Select(r => r.PupilMatchingRef));
        Assert.Equal(2, reader.RowsRead);
        Assert.Equal(
            [
                "PupilMatchingRef is missing.",
                "DOB isn't a valid date: '31/02/2015'.",
                "Adopted is 1; pupils adopted from care aren't loaded."
            ],
            Assert.Single(reader.RejectedRows).Reasons);
    }

    [Fact]
    public async Task ReadRowsAsync_EmptyDob_IsNull()
    {
        (PupilPremiumFileRow[] rows, _) = await ReadAsync(Header + "\n" + Row(dob: ""));

        PupilPremiumFileRow row = Assert.Single(rows);
        Assert.Null(row.Dob);
        Assert.Null(JsonNode.Parse(row.Data)!["DOB"]);
    }

    [Fact]
    public async Task ReadRowsAsync_Values_AreKeptAsStrings()
    {
        (PupilPremiumFileRow[] rows, _) = await ReadAsync(
            Header + "\tPupilPremiumFYStartDate\tLast.FSM\n" + Row(source: "") + "\t01/04/2024\tNULL");

        PupilPremiumFileRow row = Assert.Single(rows);
        Assert.Null(row.Source);
        JsonNode data = JsonNode.Parse(row.Data)!;
        Assert.Null(data["Source"]);
        Assert.Equal("01/04/2024", data["PupilPremiumFYStartDate"]!.GetValue<string>());
        Assert.Equal("NULL", data["Last_FSM"]!.GetValue<string>());
    }

    private static string Row(string pupilMatchingRef = "P1", string dob = "2015-02-01", string source = "C", string adopted = "0") =>
        $"{pupilMatchingRef}\t201\t3614\t100000\tA123456789012\tSmith\tAlex\t\tM\t{dob}\t{source}\t{adopted}";

    private static async Task<(PupilPremiumFileRow[] Rows, PupilPremiumDataFileReader Reader)> ReadAsync(string content)
    {
        PupilPremiumDataFileReader reader = await PupilPremiumDataFileReader.OpenAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)));

        List<PupilPremiumFileRow> rows = [];
        await foreach (PupilPremiumFileRow row in reader.ReadRowsAsync())
        {
            rows.Add(row);
        }

        return ([.. rows], reader);
    }
}
