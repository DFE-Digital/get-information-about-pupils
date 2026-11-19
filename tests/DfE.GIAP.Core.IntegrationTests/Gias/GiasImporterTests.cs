using System.Net;
using System.Text;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Gias;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.SharedTests.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DfE.GIAP.Core.IntegrationTests.Gias;

[Collection(PostgresIntegrationTestCollectionMarker.Name)]
public sealed class GiasImporterTests : IAsyncLifetime
{
    private const string AllEstablishmentDataFileName = "edubasealldata20261117.csv";
    private const string AllGroupLinksFileName = "alllinksdata20261117.csv";

    // Abridged copies of real GIAS rows; the column order differs from the parser's and there are unused columns
    private const string AllEstablishmentDataCsv =
        """
        "URN","LA (code)","LA (name)","EstablishmentNumber","EstablishmentName","TypeOfEstablishment (code)","TypeOfEstablishment (name)","EstablishmentStatus (code)","EstablishmentStatus (name)","CloseDate","PhaseOfEducation (code)","PhaseOfEducation (name)","StatutoryLowAge","StatutoryHighAge","SchoolCapacity","UPRN","GOR (code)"
        "100000","201","City of London","3614","Haute Valleé School","2","Voluntary aided school","1","Open","","2","Primary","3","11","250","200000071925","H"
        "100001","673","Pre-LGR 2019 Poole","2000","Example Welsh School","1","Community school","2","Closed","31-08-2019","2","Primary","","","","","W"
        """;

    private const string AllGroupLinksCsv =
        """
        "URN","Group UID","Group ID","Group Name","Group Type (code)","Group Type","Closed Date","Group Status (code)","Group Status","Joined date","EstablishmentName"
        "100000","1000","TR00001","Example Academy Trust","06","Multi-academy trust","","OPEN","Open","01/09/2015","Haute Valleé School"
        "20003","80637","","Central","08","Children's Centres Group","","OPEN","Open","01/01/1900","Central Sure Start Children's Centre"
        """;

    private static readonly Encoding _windows1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252)!;

    private readonly GiapPostgresFixture _postgres;
    private readonly StubHttpMessageHandler _httpHandler = new();
    private readonly GiasImporter _importer;

    public GiasImporterTests(GiapPostgresFixture postgres)
    {
        _postgres = postgres;
        _importer = new GiasImporter(
            new GiasExtractClient(new HttpClient(_httpHandler) { BaseAddress = new Uri("https://gias.example/downloads/") }),
            new GiasDataWriter(new GiapPostgresDbContextFactory(postgres)),
            new FixedTimeProvider(new DateTimeOffset(2026, 11, 17, 6, 0, 0, TimeSpan.Zero)),
            NullLogger<GiasImporter>.Instance);
    }

    public async Task InitializeAsync()
    {
        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        await dbContext.EstablishmentGroupLinks.ExecuteDeleteAsync();
        await dbContext.EstablishmentGroups.ExecuteDeleteAsync();
        await dbContext.Establishments.ExecuteDeleteAsync();
        await dbContext.LocalAuthorities.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ImportAsync_ImportsTodaysExtractsUnfiltered()
    {
        _httpHandler.Responses[AllEstablishmentDataFileName] = AllEstablishmentDataCsv;
        _httpHandler.Responses[AllGroupLinksFileName] = AllGroupLinksCsv;

        GiasImportResult result = await _importer.ImportAsync();

        Assert.Equal(new GiasImportResult(LocalAuthorityCount: 2, EstablishmentCount: 2, GroupCount: 2, GroupLinkCount: 2), result);

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        Establishment[] establishments = await dbContext.Establishments.OrderBy(e => e.Urn).ToArrayAsync();
        Assert.Equal(["Haute Valleé School", "Example Welsh School"], establishments.Select(e => e.Name));
        Assert.Equal(
            [(80637, 20003), (1000, 100000)],
            (await dbContext.EstablishmentGroupLinks.OrderBy(l => l.Urn).ToArrayAsync()).Select(l => (l.GroupUid, l.Urn)));
    }

    [Fact]
    public async Task ImportAsync_LeavesExistingDataUntouched_WhenAnExtractIsUnavailable()
    {
        _httpHandler.Responses[AllEstablishmentDataFileName] = AllEstablishmentDataCsv;
        _httpHandler.Responses[AllGroupLinksFileName] = AllGroupLinksCsv;
        await _importer.ImportAsync();

        _httpHandler.Responses.Remove(AllGroupLinksFileName);
        _httpHandler.Responses[AllEstablishmentDataFileName] = AllEstablishmentDataCsv.Replace("Haute Valleé School", "Renamed School");

        await Assert.ThrowsAsync<HttpRequestException>(() => _importer.ImportAsync());

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        Assert.Equal("Haute Valleé School", (await dbContext.Establishments.SingleAsync(e => e.Urn == 100000)).Name);
    }

    [Fact]
    public async Task ImportAsync_LeavesExistingDataUntouched_WhenAnExtractIsMalformed()
    {
        _httpHandler.Responses[AllEstablishmentDataFileName] = AllEstablishmentDataCsv;
        _httpHandler.Responses[AllGroupLinksFileName] = AllGroupLinksCsv;
        await _importer.ImportAsync();

        _httpHandler.Responses[AllEstablishmentDataFileName] = AllEstablishmentDataCsv.Replace("Haute Valleé School", "Renamed School");
        _httpHandler.Responses[AllGroupLinksFileName] = AllGroupLinksCsv.Replace("\"Group UID\",", "");

        await Assert.ThrowsAsync<GiasExtractException>(() => _importer.ImportAsync());

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        Assert.Equal("Haute Valleé School", (await dbContext.Establishments.SingleAsync(e => e.Urn == 100000)).Name);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        public Dictionary<string, string> Responses { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string fileName = request.RequestUri!.Segments[^1];

            HttpResponseMessage response = Responses.TryGetValue(fileName, out string? csv)
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(_windows1252.GetBytes(csv)) }
                : new HttpResponseMessage(HttpStatusCode.NotFound);

            return Task.FromResult(response);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
