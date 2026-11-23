using System.Runtime.CompilerServices;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Verification.Comparison;
using Microsoft.EntityFrameworkCore;

namespace DfE.GIAP.Verification.Sources;

/// <summary>
/// Reads the pupils or learners the new pipeline wrote to Postgres for one of the old pipeline's containers.
/// </summary>
public sealed class PostgresRecordSource(IDbContextFactory<GiapDbContext> dbContextFactory, RecordContainer container)
{
    /// <summary>
    /// Every key, in batches.
    /// </summary>
    public async IAsyncEnumerable<string[]> ReadKeyBatchesAsync(int batchSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? lastKey = null;

        while (true)
        {
            string[] batch;
            await using (GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                batch = await Keys(dbContext)
                    .Where(k => lastKey == null || string.Compare(k, lastKey) > 0)
                    .OrderBy(k => k)
                    .Take(batchSize)
                    .ToArrayAsync(cancellationToken);
            }

            if (batch.Length == 0)
            {
                yield break;
            }

            yield return batch;
            lastKey = batch[^1];
        }
    }

    public async Task<IReadOnlySet<string>> FindExistingAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default)
    {
        await using GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await Keys(dbContext).Where(k => keys.Contains(k)).ToHashSetAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, ComparedRecord>> ReadAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default)
    {
        await using GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (container == RecordContainer.NationalPupilDatabase)
        {
            List<NpdPupil> pupils = await dbContext.NpdPupils.AsNoTracking()
                .Where(p => keys.Contains(p.PupilMatchingRef))
                .ToListAsync(cancellationToken);
            ILookup<string, NpdPupilRecord> records = (await dbContext.NpdPupilRecords.AsNoTracking()
                .Where(r => keys.Contains(r.PupilMatchingRef))
                .ToListAsync(cancellationToken))
                .ToLookup(r => r.PupilMatchingRef);

            return pupils.ToDictionary(p => p.PupilMatchingRef, p => RecordMappings.FromNpd(p, records[p.PupilMatchingRef]));
        }

        if (container == RecordContainer.PupilPremium)
        {
            return await dbContext.PupilPremiumPupils.AsNoTracking()
                .Where(p => keys.Contains(p.PupilMatchingRef))
                .ToDictionaryAsync(p => p.PupilMatchingRef, RecordMappings.FromPupilPremium, cancellationToken);
        }

        List<FurtherEducationLearner> learners = await dbContext.FurtherEducationLearners.AsNoTracking()
            .Where(l => keys.Contains(l.Uln))
            .ToListAsync(cancellationToken);
        ILookup<string, FurtherEducationLearnerRecord> learnerRecords = (await dbContext.FurtherEducationLearnerRecords.AsNoTracking()
            .Where(r => keys.Contains(r.Uln))
            .ToListAsync(cancellationToken))
            .ToLookup(r => r.Uln);

        return learners.ToDictionary(l => l.Uln, l => RecordMappings.FromFurtherEducation(l, learnerRecords[l.Uln]));
    }

    private IQueryable<string> Keys(GiapDbContext dbContext) =>
        container == RecordContainer.NationalPupilDatabase ? dbContext.NpdPupils.Select(p => p.PupilMatchingRef)
        : container == RecordContainer.PupilPremium ? dbContext.PupilPremiumPupils.Select(p => p.PupilMatchingRef)
        : dbContext.FurtherEducationLearners.Select(l => l.Uln);
}
