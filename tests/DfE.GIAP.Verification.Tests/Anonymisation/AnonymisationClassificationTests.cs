using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Verification.Anonymisation;

namespace DfE.GIAP.Verification.Tests.Anonymisation;

public class AnonymisationClassificationTests
{
    [Fact]
    public void LoadShipped_IsValid()
    {
        AnonymisationClassification classification = AnonymisationClassification.LoadShipped();

        Assert.NotEmpty(classification.PmrFormats);
        Assert.Equal(ColumnTransform.Pmr, classification.GetFolder(IngestFolder.Get("ks2"))!.GetTransform("pupilmatchingref"));
    }

    [Fact]
    public void Parse_ReadsTheTransformsAndKeptColumns()
    {
        AnonymisationClassification classification = AnonymisationClassification.Parse(
            """
            {
                "identifiers": { "pmr": [{ "source": "[0-9A-F]{18}", "target": "[G-V][0-9A-F]{17}" }] },
                "folders": {
                    "ks2": {
                        "subject": "PupilMatchingRef",
                        "sex": "GENDER",
                        "transform": { "PupilMatchingRef": "pmr", "CANDNO": "candidate-number", "MiddleNames": "middle-names" },
                        "keep": ["GENDER", "LA"]
                    }
                }
            }
            """);

        FolderClassification ks2 = classification.GetFolder(IngestFolder.Get("ks2"))!;
        Assert.Equal(ColumnTransform.CandidateNumber, ks2.GetTransform("CANDNO"));
        Assert.Equal(ColumnTransform.MiddleNames, ks2.GetTransform("middlenames"));
        Assert.Equal(ColumnTransform.Keep, ks2.GetTransform("LA"));
        Assert.Null(ks2.GetTransform("Address"));
        Assert.Null(classification.GetFolder(IngestFolder.Get("ks4")));
    }

    [Theory]
    // The reserved range is smaller than the real one
    [InlineData("""{ "identifiers": { "pmr": [{ "source": "[0-9A-F]{18}", "target": "[G-V][0-9A-F]{16}" }] } }""")]
    // The reserved range overlaps the real one
    [InlineData("""{ "identifiers": { "pmr": [{ "source": "[0-9A-F]{18}", "target": "[A-Z][0-9A-F]{17}" }] } }""")]
    // Two reserved ranges overlap
    [InlineData("""{ "identifiers": { "pmr": [{ "source": "[0-9]{18}", "target": "[G-V][0-9A-F]{17}" }, { "source": "[A-F]{18}", "target": "[G-V][0-9A-F]{17}" }] } }""")]
    [InlineData("""{ "folders": { "nope": { "keep": [] } } }""")]
    [InlineData("""{ "folders": { "ks2": { "transform": { "LA": "pmr" } } } }""")]
    [InlineData("""{ "folders": { "ks2": { "transform": { "DOB": "dob" }, "keep": ["dob"] } } }""")]
    [InlineData("""{ "folders": { "ks2": { "subject": "LA", "keep": ["LA"] } } }""")]
    [InlineData("""{ "folders": { "ks2": { "sex": "GENDER", "transform": { "GENDER": "blank" } } } }""")]
    [InlineData("""{ "folders": { "ks2": { "transform": { "DOB": "shuffle" } } } }""")]
    public void Parse_AnInvalidClassification_Throws(string json) =>
        Assert.ThrowsAny<Exception>(() => AnonymisationClassification.Parse(json));
}
