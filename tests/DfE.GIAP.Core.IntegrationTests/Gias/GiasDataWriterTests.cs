using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Gias;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.SharedTests.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace DfE.GIAP.Core.IntegrationTests.Gias;

[Collection(PostgresIntegrationTestCollectionMarker.Name)]
public sealed class GiasDataWriterTests(GiapPostgresFixture postgres) : IAsyncLifetime
{
    private readonly GiasDataWriter _writer = new(new GiapPostgresDbContextFactory(postgres));

    public async Task InitializeAsync()
    {
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        await dbContext.EstablishmentGroupLinks.ExecuteDeleteAsync();
        await dbContext.EstablishmentGroups.ExecuteDeleteAsync();
        await dbContext.Establishments.ExecuteDeleteAsync();
        await dbContext.LocalAuthorities.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ReplaceAllAsync_WritesAllRows()
    {
        Establishment establishment = new()
        {
            Urn = 100000,
            LaCode = 201,
            EstablishmentNumber = null,
            Name = "Haute Valleé School",
            Uprn = 200000071925L,
            TypeOfEstablishmentCode = 2,
            TypeOfEstablishmentName = "Voluntary aided school",
            EstablishmentStatusCode = 2,
            EstablishmentStatusName = "Closed",
            CloseDate = new DateOnly(2021, 8, 31),
            PhaseOfEducationCode = 2,
            PhaseOfEducationName = "Primary",
            StatutoryLowAge = null,
            StatutoryHighAge = 11,
            GorCode = null
        };
        EstablishmentGroup group = new()
        {
            GroupUid = 1000,
            GroupId = null,
            Name = "Example Federation",
            GroupTypeCode = 1,
            GroupTypeName = "Federation",
            GroupStatusCode = "CLOSED",
            GroupStatusName = "Closed",
            ClosedDate = new DateOnly(2020, 8, 31)
        };
        // URN 999999 isn't in the establishments extract, which happens in real GIAS data
        EstablishmentGroupLink link = new() { GroupUid = 1000, Urn = 999999, JoinedDate = null };

        GiasImportResult result = await _writer.ReplaceAllAsync(
            new GiasEstablishmentsExtract([CreateLocalAuthority(201)], [establishment]),
            new GiasGroupLinksExtract([group], [link]));

        Assert.Equal(new GiasImportResult(1, 1, 1, 1), result);

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equivalent(CreateLocalAuthority(201), await dbContext.LocalAuthorities.SingleAsync());
        Assert.Equivalent(establishment, await dbContext.Establishments.SingleAsync());
        Assert.Equivalent(group, await dbContext.EstablishmentGroups.SingleAsync());
        Assert.Equivalent(link, await dbContext.EstablishmentGroupLinks.SingleAsync());
    }

    [Fact]
    public async Task ReplaceAllAsync_ReplacesExistingRows()
    {
        await _writer.ReplaceAllAsync(CreateEstablishmentsExtract(100000, 100001), CreateGroupLinksExtract((1000, 100000), (1000, 100001)));

        await _writer.ReplaceAllAsync(CreateEstablishmentsExtract(100001, 100002), CreateGroupLinksExtract((2000, 100001), (2000, 100002)));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(new[] { 100001, 100002 }, await dbContext.Establishments.Select(e => e.Urn).OrderBy(u => u).ToArrayAsync());
        Assert.Equal(new[] { 2000 }, await dbContext.EstablishmentGroups.Select(g => g.GroupUid).ToArrayAsync());
        Assert.Equal(
            new[] { (2000, 100001), (2000, 100002) },
            (await dbContext.EstablishmentGroupLinks.OrderBy(l => l.Urn).ToArrayAsync()).Select(l => (l.GroupUid, l.Urn)));
    }

    [Fact]
    public async Task ReplaceAllAsync_ThrowsAndLeavesExistingRows_WhenEstablishmentsExtractHasFarFewerRows()
    {
        int[] existingUrns = [.. Enumerable.Range(100000, 10)];
        await _writer.ReplaceAllAsync(CreateEstablishmentsExtract(existingUrns), CreateGroupLinksExtract((1000, 100000)));

        GiasExtractException ex = await Assert.ThrowsAsync<GiasExtractException>(() =>
            _writer.ReplaceAllAsync(CreateEstablishmentsExtract([.. existingUrns.Take(8)]), CreateGroupLinksExtract((1000, 100000))));

        Assert.Equal(GiasExtractType.AllEstablishmentData, ex.ExtractType);
        await AssertEstablishmentUrnsAsync(existingUrns);
    }

    [Fact]
    public async Task ReplaceAllAsync_ThrowsAndLeavesExistingRows_WhenGroupLinksExtractHasFarFewerRows()
    {
        int[] existingUrns = [.. Enumerable.Range(100000, 10)];
        await _writer.ReplaceAllAsync(
            CreateEstablishmentsExtract(existingUrns),
            CreateGroupLinksExtract([.. existingUrns.Select(urn => (1000, urn))]));

        GiasExtractException ex = await Assert.ThrowsAsync<GiasExtractException>(() =>
            _writer.ReplaceAllAsync(CreateEstablishmentsExtract(existingUrns), CreateGroupLinksExtract((1000, 100000))));

        Assert.Equal(GiasExtractType.AllGroupLinks, ex.ExtractType);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(existingUrns.Length, await dbContext.EstablishmentGroupLinks.CountAsync());
    }

    [Fact]
    public async Task ReplaceAllAsync_AcceptsASmallDropInRows()
    {
        int[] existingUrns = [.. Enumerable.Range(100000, 10)];
        await _writer.ReplaceAllAsync(CreateEstablishmentsExtract(existingUrns), CreateGroupLinksExtract((1000, 100000)));

        await _writer.ReplaceAllAsync(CreateEstablishmentsExtract([.. existingUrns.Take(9)]), CreateGroupLinksExtract((1000, 100000)));

        await AssertEstablishmentUrnsAsync([.. existingUrns.Take(9)]);
    }

    [Fact]
    public async Task ReplaceAllAsync_RollsBack_WhenWritingFails()
    {
        await _writer.ReplaceAllAsync(CreateEstablishmentsExtract(100000), CreateGroupLinksExtract((1000, 100000)));

        // The link's group isn't in the extract, so the foreign key is violated after the existing rows are deleted
        GiasGroupLinksExtract invalidGroupLinks = new(
            [CreateGroup(2000)],
            [new EstablishmentGroupLink { GroupUid = 3000, Urn = 100001, JoinedDate = null }]);

        await Assert.ThrowsAsync<PostgresException>(() =>
            _writer.ReplaceAllAsync(CreateEstablishmentsExtract(100001), invalidGroupLinks));

        await AssertEstablishmentUrnsAsync([100000]);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(new[] { 1000 }, await dbContext.EstablishmentGroups.Select(g => g.GroupUid).ToArrayAsync());
    }

    [Fact]
    public async Task ReplaceAllAsync_DoesNotBlockReaders()
    {
        await _writer.ReplaceAllAsync(CreateEstablishmentsExtract(100000), CreateGroupLinksExtract((1000, 100000)));

        // Hold an uncommitted replace open, as if the import were part-way through, and read from another connection
        await using GiapDbContext writerContext = postgres.CreateDbContext();
        await using IDbContextTransaction transaction = await writerContext.Database.BeginTransactionAsync();
        await writerContext.EstablishmentGroupLinks.ExecuteDeleteAsync();
        await writerContext.Establishments.ExecuteDeleteAsync();

        await using GiapDbContext readerContext = postgres.CreateDbContext();
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        int[] urns = await readerContext.Establishments.Select(e => e.Urn).ToArrayAsync(timeout.Token);

        Assert.Equal([100000], urns);
    }

    private async Task AssertEstablishmentUrnsAsync(int[] expectedUrns)
    {
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(expectedUrns, await dbContext.Establishments.Select(e => e.Urn).OrderBy(u => u).ToArrayAsync());
    }

    private static GiasEstablishmentsExtract CreateEstablishmentsExtract(params int[] urns) =>
        new([CreateLocalAuthority(201)], [.. urns.Select(CreateEstablishment)]);

    private static GiasGroupLinksExtract CreateGroupLinksExtract(params (int GroupUid, int Urn)[] links) =>
        new(
            [.. links.Select(l => l.GroupUid).Distinct().Select(CreateGroup)],
            [.. links.Select(l => new EstablishmentGroupLink { GroupUid = l.GroupUid, Urn = l.Urn, JoinedDate = new DateOnly(2015, 9, 1) })]);

    private static LocalAuthority CreateLocalAuthority(int laCode) => new() { LaCode = laCode, Name = $"Local authority {laCode}" };

    private static Establishment CreateEstablishment(int urn) =>
        new()
        {
            Urn = urn,
            LaCode = 201,
            EstablishmentNumber = urn % 10000,
            Name = $"School {urn}",
            Uprn = null,
            TypeOfEstablishmentCode = 2,
            TypeOfEstablishmentName = "Voluntary aided school",
            EstablishmentStatusCode = 1,
            EstablishmentStatusName = "Open",
            CloseDate = null,
            PhaseOfEducationCode = 2,
            PhaseOfEducationName = "Primary",
            StatutoryLowAge = 3,
            StatutoryHighAge = 11,
            GorCode = "H"
        };

    private static EstablishmentGroup CreateGroup(int groupUid) =>
        new()
        {
            GroupUid = groupUid,
            GroupId = $"TR{groupUid:D5}",
            Name = $"Academy trust {groupUid}",
            GroupTypeCode = EstablishmentGroup.MultiAcademyTrustGroupTypeCode,
            GroupTypeName = "Multi-academy trust",
            GroupStatusCode = EstablishmentGroup.OpenGroupStatusCode,
            GroupStatusName = "Open",
            ClosedDate = null
        };
}
