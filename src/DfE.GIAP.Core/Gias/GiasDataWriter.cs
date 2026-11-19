using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace DfE.GIAP.Core.Gias;

public class GiasDataWriter(IDbContextFactory<GiapDbContext> dbContextFactory)
{
    public const double MinimumRowCountRatio = 0.9;

    // Key for the advisory lock that stops two imports from running at once ('gias' in ASCII)
    private const long AdvisoryLockKey = 0x67696173;

    /// <remarks>
    /// All the data is replaced in a single transaction. Rows are deleted rather than truncated so that readers
    /// aren't blocked; they continue to see the previous data until the transaction commits.
    /// </remarks>
    /// <exception cref="GiasExtractException">
    /// An extract has far fewer rows than the data currently stored. The existing data is left untouched.
    /// </exception>
    public async Task<GiasImportResult> ReplaceAllAsync(
        GiasEstablishmentsExtract establishments,
        GiasGroupLinksExtract groupLinks,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(establishments);
        ArgumentNullException.ThrowIfNull(groupLinks);

        await using GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        NpgsqlConnection connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();

        await dbContext.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({AdvisoryLockKey})", cancellationToken);

        int currentEstablishmentCount = await dbContext.Establishments.CountAsync(cancellationToken);
        EnsureRowCountIsPlausible(GiasExtractType.AllEstablishmentData, establishments.Establishments.Count, currentEstablishmentCount);

        int currentLinkCount = await dbContext.EstablishmentGroupLinks.CountAsync(cancellationToken);
        EnsureRowCountIsPlausible(GiasExtractType.AllGroupLinks, groupLinks.Links.Count, currentLinkCount);

        // Children before parents, to satisfy the foreign keys
        await dbContext.EstablishmentGroupLinks.ExecuteDeleteAsync(cancellationToken);
        await dbContext.EstablishmentGroups.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Establishments.ExecuteDeleteAsync(cancellationToken);
        await dbContext.LocalAuthorities.ExecuteDeleteAsync(cancellationToken);

        // Parents before children
        await CopyAsync(dbContext, connection, establishments.LocalAuthorities, cancellationToken);
        await CopyAsync(dbContext, connection, establishments.Establishments, cancellationToken);
        await CopyAsync(dbContext, connection, groupLinks.Groups, cancellationToken);
        await CopyAsync(dbContext, connection, groupLinks.Links, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new GiasImportResult(
            LocalAuthorityCount: establishments.LocalAuthorities.Count,
            EstablishmentCount: establishments.Establishments.Count,
            GroupCount: groupLinks.Groups.Count,
            GroupLinkCount: groupLinks.Links.Count);
    }

    private static void EnsureRowCountIsPlausible(GiasExtractType extractType, int newCount, int currentCount)
    {
        if (newCount < currentCount * MinimumRowCountRatio)
        {
            throw new GiasExtractException(
                extractType,
                $"The extract has {newCount} rows, far fewer than the {currentCount} rows currently stored.");
        }
    }

    // Writes the rows with a binary COPY, taking the table and columns from the EF Core model
    private static async Task CopyAsync<T>(
        GiapDbContext dbContext,
        NpgsqlConnection connection,
        IEnumerable<T> rows,
        CancellationToken cancellationToken)
        where T : class
    {
        IEntityType entityType = dbContext.Model.FindEntityType(typeof(T))!;
        StoreObjectIdentifier table = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
        IProperty[] properties = [.. entityType.GetProperties()];

        string columnList = string.Join(", ", properties.Select(p => p.GetColumnName(table)));
        string copyCommand = $"COPY {table.DisplayName()} ({columnList}) FROM STDIN (FORMAT BINARY)";

        await using NpgsqlBinaryImporter importer = await connection.BeginBinaryImportAsync(copyCommand, cancellationToken);

        foreach (T row in rows)
        {
            await importer.StartRowAsync(cancellationToken);

            foreach (IProperty property in properties)
            {
                object? value = property.GetGetter().GetClrValueUsingContainingEntity(row);

                if (value is null)
                {
                    await importer.WriteNullAsync(cancellationToken);
                }
                else
                {
                    await importer.WriteAsync(value, property.GetColumnType(), cancellationToken);
                }
            }
        }

        await importer.CompleteAsync(cancellationToken);
    }
}

public sealed record GiasImportResult(int LocalAuthorityCount, int EstablishmentCount, int GroupCount, int GroupLinkCount);
