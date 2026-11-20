using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.PupilData;
using DfE.GIAP.SharedTests.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DfE.GIAP.Core.IntegrationTests.PupilData;

[Collection(PostgresIntegrationTestCollectionMarker.Name)]
public sealed class FurtherEducationDataWriterTests(GiapPostgresFixture postgres) : IAsyncLifetime
{
    private const string AcademicYear = "2024/2025";

    private readonly FurtherEducationDataWriter _writer = new(new GiapPostgresDbContextFactory(postgres));

    public async Task InitializeAsync()
    {
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        await dbContext.FurtherEducationLearnerRecords.ExecuteDeleteAsync();
        await dbContext.FurtherEducationLearners.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData(FurtherEducationDataset.PupilPremium)]
    [InlineData(FurtherEducationDataset.SpecialEducationalNeeds)]
    public async Task WriteFileAsync_WritesRecordsAndLearners(FurtherEducationDataset dataset)
    {
        FurtherEducationFileWriteResult result = await WriteFileAsync(dataset, CreateRow(1, "1", data: """{"NCYear": "12"}"""));

        Assert.Equal(new FurtherEducationFileWriteResult(RecordsWritten: 1, RecordsRemoved: 0), result);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        FurtherEducationLearnerRecord record = await dbContext.FurtherEducationLearnerRecords.SingleAsync();
        Assert.Equivalent(
            new FurtherEducationLearnerRecord
            {
                Uln = "1",
                Dataset = dataset,
                AcademicYear = AcademicYear,
                Surname = "Smith",
                Forename = "Alex",
                Sex = "M",
                Dob = new DateOnly(2005, 2, 1),
                Data = """[{"NCYear": "12"}]""",
                CreatedAt = record.CreatedAt
            },
            record);
        Assert.Equivalent(
            new FurtherEducationLearner
            {
                Uln = "1",
                Surname = "Smith",
                Forename = "Alex",
                Sex = "M",
                Dob = new DateOnly(2005, 2, 1)
            },
            await dbContext.FurtherEducationLearners.SingleAsync());
    }

    [Fact]
    public async Task WriteFileAsync_LearnerWithMoreThanOneRow_CombinesRowsInFileOrderWithFieldsFromFirstRow()
    {
        FurtherEducationFileWriteResult result = await WriteFileAsync(
            FurtherEducationDataset.SpecialEducationalNeeds,
            CreateRow(1, "1", surname: "First", data: """{"Row": "1"}"""),
            CreateRow(2, "2", data: """{"Row": "2"}"""),
            CreateRow(3, "1", surname: "Second", data: """{"Row": "3"}"""));

        Assert.Equal(2, result.RecordsWritten);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        FurtherEducationLearnerRecord record = await dbContext.FurtherEducationLearnerRecords.SingleAsync(r => r.Uln == "1");
        Assert.Equal("First", record.Surname);
        Assert.Equal("""[{"Row": "1"}, {"Row": "3"}]""", record.Data);
    }

    [Fact]
    public async Task WriteFileAsync_PupilPremiumFile_ReplacesAllFurtherEducationData()
    {
        await _writer.WriteFileAsync(FurtherEducationDataset.PupilPremium, "2023/2024", ToAsync(CreateRow(1, "1"), CreateRow(2, "2")));
        await WriteFileAsync(FurtherEducationDataset.SpecialEducationalNeeds, CreateRow(1, "2"), CreateRow(2, "3"));

        FurtherEducationFileWriteResult result = await WriteFileAsync(
            FurtherEducationDataset.PupilPremium,
            CreateRow(1, "2", surname: "New"),
            CreateRow(2, "4"));

        // Both 2023/2024 pupil premium records and both SEN records are removed
        Assert.Equal(new FurtherEducationFileWriteResult(RecordsWritten: 2, RecordsRemoved: 4), result);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(
            [("2", FurtherEducationDataset.PupilPremium, AcademicYear, "New"), ("4", FurtherEducationDataset.PupilPremium, AcademicYear, "Smith")],
            (await dbContext.FurtherEducationLearnerRecords.OrderBy(r => r.Uln).ToArrayAsync())
                .Select(r => (r.Uln, r.Dataset, r.AcademicYear, r.Surname)));
        Assert.Equal(
            [("2", "New"), ("4", "Smith")],
            (await dbContext.FurtherEducationLearners.OrderBy(l => l.Uln).ToArrayAsync()).Select(l => (l.Uln, l.Surname)));
    }

    [Fact]
    public async Task WriteFileAsync_SenFile_ReplacesSenRecordsForItsYearOnly()
    {
        await WriteFileAsync(FurtherEducationDataset.PupilPremium, CreateRow(1, "1"));
        await WriteFileAsync(FurtherEducationDataset.SpecialEducationalNeeds, CreateRow(1, "1"), CreateRow(2, "2"), CreateRow(3, "3"));
        await _writer.WriteFileAsync(FurtherEducationDataset.SpecialEducationalNeeds, "2023/2024", ToAsync(CreateRow(1, "3")));

        FurtherEducationFileWriteResult result = await WriteFileAsync(
            FurtherEducationDataset.SpecialEducationalNeeds,
            CreateRow(1, "4"));

        Assert.Equal(new FurtherEducationFileWriteResult(RecordsWritten: 1, RecordsRemoved: 3), result);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(
            [
                ("1", FurtherEducationDataset.PupilPremium, AcademicYear),
                ("3", FurtherEducationDataset.SpecialEducationalNeeds, "2023/2024"),
                ("4", FurtherEducationDataset.SpecialEducationalNeeds, AcademicYear)
            ],
            (await dbContext.FurtherEducationLearnerRecords.OrderBy(r => r.Uln).ToArrayAsync())
                .Select(r => (r.Uln, r.Dataset, r.AcademicYear)));
        // 2 has no records left
        Assert.Equal(["1", "3", "4"], await dbContext.FurtherEducationLearners.OrderBy(l => l.Uln).Select(l => l.Uln).ToArrayAsync());
    }

    [Fact]
    public async Task WriteFileAsync_LearnerFields_ComeFromPupilPremiumRecordBeforeLaterRecords()
    {
        await _writer.WriteFileAsync(FurtherEducationDataset.PupilPremium, "2023/2024", ToAsync(CreateRow(1, "1", surname: "PupilPremium")));
        await WriteFileAsync(FurtherEducationDataset.SpecialEducationalNeeds, CreateRow(1, "1", surname: "Sen"));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal("PupilPremium", (await dbContext.FurtherEducationLearners.SingleAsync()).Surname);
    }

    [Fact]
    public async Task WriteFileAsync_LearnerFields_ComeFromLatestAcademicYear()
    {
        await WriteFileAsync(FurtherEducationDataset.SpecialEducationalNeeds, CreateRow(1, "1", surname: "Later"));
        await _writer.WriteFileAsync(FurtherEducationDataset.SpecialEducationalNeeds, "2023/2024", ToAsync(CreateRow(1, "1", surname: "Earlier")));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal("Later", (await dbContext.FurtherEducationLearners.SingleAsync()).Surname);
    }

    [Fact]
    public async Task WriteFileAsync_ReplacedRecord_KeepsCreatedAt()
    {
        await WriteFileAsync(FurtherEducationDataset.SpecialEducationalNeeds, CreateRow(1, "1"));
        DateTime createdAt = await GetCreatedAtAsync();

        await WriteFileAsync(FurtherEducationDataset.SpecialEducationalNeeds, CreateRow(1, "1", surname: "New"));

        Assert.Equal(createdAt, await GetCreatedAtAsync());

        async Task<DateTime> GetCreatedAtAsync()
        {
            await using GiapDbContext dbContext = postgres.CreateDbContext();
            return (await dbContext.FurtherEducationLearnerRecords.SingleAsync()).CreatedAt;
        }
    }

    [Fact]
    public async Task WriteFileAsync_NoRows_ThrowsAndLeavesStoredRecords()
    {
        await WriteFileAsync(FurtherEducationDataset.PupilPremium, CreateRow(1, "1"));

        await Assert.ThrowsAsync<ArgumentException>(() => WriteFileAsync(FurtherEducationDataset.PupilPremium));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(1, await dbContext.FurtherEducationLearnerRecords.CountAsync());
    }

    private Task<FurtherEducationFileWriteResult> WriteFileAsync(FurtherEducationDataset dataset, params FurtherEducationFileRow[] rows) =>
        _writer.WriteFileAsync(dataset, AcademicYear, ToAsync(rows));

    private static async IAsyncEnumerable<FurtherEducationFileRow> ToAsync(params FurtherEducationFileRow[] rows)
    {
        foreach (FurtherEducationFileRow row in rows)
        {
            await Task.Yield();
            yield return row;
        }
    }

    private static FurtherEducationFileRow CreateRow(int rowNumber, string uln, string surname = "Smith", string data = "{}") =>
        new(
            rowNumber,
            uln,
            Surname: surname,
            Forename: "Alex",
            Sex: "M",
            Dob: new DateOnly(2005, 2, 1),
            Data: data);
}
