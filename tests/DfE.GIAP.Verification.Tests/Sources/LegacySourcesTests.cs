using System.Text;
using System.Text.Json.Nodes;
using DfE.GIAP.Verification.Sources;

namespace DfE.GIAP.Verification.Tests.Sources;

public class LegacySourcesTests
{
    [Fact]
    public async Task ParseRejectedRowsAsync_DropsPandasIndexAndReadsMissingValuesAsNull()
    {
        // As written by pandas' to_csv
        const string csv =
            ",PupilMatchingRef,DOB,ACADYR\n" +
            "0,PMR1,not a date,2024/2025\n" +
            "3,nan,2010-03-01,\n";

        IReadOnlyList<JsonObject> rows = await LegacyProcessingStorage.ParseRejectedRowsAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)));

        Assert.Equal(
            [
                """{"PupilMatchingRef":"PMR1","DOB":"not a date","ACADYR":"2024/2025"}""",
                """{"PupilMatchingRef":null,"DOB":"2010-03-01","ACADYR":null}"""
            ],
            rows.Select(r => r.ToJsonString()));
    }

    [Fact]
    public void FromDocument_ReadsTheOldPipelinesFileResult()
    {
        JsonObject document = JsonNode.Parse(
            """
            {
                "id": "20250102030405",
                "DOCTYPE": "KS2",
                "File Name": "NPD_KS2_2025_U/NPD_KS2_2025_U_KtS.txt",
                "Number Of Source Records": 10,
                "Number Of Failed QC Records": 1,
                "Number Of Passed QC Records": 9,
                "Number Of Unique Records": 8,
                "Start DateTime": "2025-01-02-02:59:00",
                "End DateTime": "2025-01-02-03:04:05"
            }
            """)!.AsObject();

        LegacyFileResult? result = LegacyFileResult.FromDocument(document);

        Assert.NotNull(result);
        Assert.Equal("NPD_KS2_2025_U_KtS.txt", result.FileName);
        Assert.Equal("1", result.FailedQcRecords!.ToJsonString());
        Assert.Equal(new DateTime(2025, 1, 2, 2, 59, 0, DateTimeKind.Utc), result.Started);
        Assert.Equal(DateTimeKind.Utc, result.Ended!.Value.Kind);
    }

    [Fact]
    public void FromDocument_GiasDocument_IsNull() =>
        Assert.Null(LegacyFileResult.FromDocument(JsonNode.Parse("""{"id": "201", "DOCTYPE": "LA"}""")!.AsObject()));
}
