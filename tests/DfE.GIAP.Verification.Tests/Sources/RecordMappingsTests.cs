using System.Text.Json.Nodes;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Verification.Comparison;
using DfE.GIAP.Verification.Sources;

namespace DfE.GIAP.Verification.Tests.Sources;

public class RecordMappingsTests
{
    [Fact]
    public void FromNpd_AndFromCosmosDocument_OfTheSamePupil_HaveNoDifferences()
    {
        NpdPupil pupil = new()
        {
            PupilMatchingRef = "PMR1",
            Upn = "A123456789012",
            LaCode = 201,
            EstablishmentNumber = 3614,
            Urn = 100000,
            Surname = "Smith",
            Forename = "Alex",
            MiddleNames = null,
            Sex = "F",
            Dob = new DateOnly(2010, 3, 1)
        };
        NpdPupilRecord[] records =
        [
            Record(NpdDataset.Ks2, "2023/2024", """[{"PupilMatchingRef": "PMR1", "ACADYR": "2023/2024"}]"""),
            Record(NpdDataset.CensusAutumn, "2024/2025", """[{"PupilMatchingRef": "PMR1", "AcademicYear": "2024/2025", "Estab": "3614"}]""")
        ];

        JsonObject document = JsonNode.Parse(
            """
            {
                "id": "PMR1",
                "_rid": "abc",
                "_ts": 1700000000,
                "PupilMatchingRef": "PMR1",
                "UPN": "A123456789012",
                "LA": 201,
                "Estab": 3614,
                "URN": 100000,
                "Surname": "Smith",
                "Forename": "Alex",
                "Sex": "F",
                "DOB": "2010-03-01",
                "KS2": [{"PupilMatchingRef": "PMR1", "ACADYR": "2023/2024"}],
                "Census_Autumn": [{"PupilMatchingRef": "PMR1", "AcademicYear": "2024/2025", "Estab": "3614"}]
            }
            """)!.AsObject();

        Assert.Empty(RecordComparer.Compare(
            "pupil-noskill",
            "PMR1",
            RecordMappings.FromNpd(pupil, records),
            RecordMappings.FromCosmosDocument(document, RecordContainer.NationalPupilDatabase)));
    }

    [Fact]
    public void FromNpd_RowsAreInAcademicYearOrder()
    {
        NpdPupil pupil = new()
        {
            PupilMatchingRef = "PMR1",
            Upn = null,
            LaCode = null,
            EstablishmentNumber = null,
            Urn = null,
            Surname = null,
            Forename = null,
            MiddleNames = null,
            Sex = null,
            Dob = null
        };

        ComparedRecord record = RecordMappings.FromNpd(
            pupil,
            [
                Record(NpdDataset.Ks2, "2024/2025", """[{"ACADYR": "2024/2025"}]"""),
                Record(NpdDataset.Ks2, "2023/2024", """[{"ACADYR": "2023/2024"}, {"ACADYR": "2023/2024", "n": "2"}]""")
            ]);

        Assert.Equal(
            ["\"2023/2024\"", "\"2023/2024\"", "\"2024/2025\""],
            record.Datasets["KS2"].Select(r => r["ACADYR"]!.ToJsonString()));
    }

    [Fact]
    public void FromCosmosDocument_FieldThatIsntADataset_IsAField()
    {
        JsonObject document = JsonNode.Parse("""{"ULN": "1234567890", "ConcatenatedName": "Alex Smith", "SEN": [{"ULN": "1234567890"}]}""")!.AsObject();

        ComparedRecord record = RecordMappings.FromCosmosDocument(document, RecordContainer.FurtherEducation);

        Assert.Equal("1234567890", record.Key);
        Assert.Equal(["ULN", "ConcatenatedName"], record.Fields.Keys);
        Assert.Equal(["SEN"], record.Datasets.Keys);
    }

    private static NpdPupilRecord Record(NpdDataset dataset, string academicYear, string data) =>
        new()
        {
            PupilMatchingRef = "PMR1",
            Dataset = dataset,
            AcademicYear = academicYear,
            IngestVersion = IngestVersion.Final,
            Upn = null,
            LaCode = null,
            EstablishmentNumber = null,
            Urn = null,
            Surname = null,
            Forename = null,
            MiddleNames = null,
            Sex = null,
            Dob = null,
            Data = data
        };
}
