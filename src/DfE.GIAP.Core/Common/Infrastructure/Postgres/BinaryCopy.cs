using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres;

internal static class BinaryCopy
{
    // Writes the rows with a binary COPY, taking the table and columns from the EF Core model; columns with a database
    // default are left for the database to fill. Pass tableName to write to another table with the same columns,
    // e.g. a staging table.
    public static async Task CopyAsync<T>(
        GiapDbContext dbContext,
        IEnumerable<T> rows,
        string? tableName = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        NpgsqlConnection connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        IEntityType entityType = dbContext.Model.FindEntityType(typeof(T))!;
        StoreObjectIdentifier table = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
        IProperty[] properties = [.. entityType.GetProperties().Where(p => p.GetDefaultValueSql() is null)];
        ValueConverter?[] converters = [.. properties.Select(p => p.GetTypeMapping().Converter)];

        string columnList = string.Join(", ", properties.Select(p => p.GetColumnName(table)));
        string copyCommand = $"COPY {tableName ?? table.DisplayName()} ({columnList}) FROM STDIN (FORMAT BINARY)";

        await using NpgsqlBinaryImporter importer = await connection.BeginBinaryImportAsync(copyCommand, cancellationToken);

        foreach (T row in rows)
        {
            await importer.StartRowAsync(cancellationToken);

            for (int i = 0; i < properties.Length; i++)
            {
                object? value = properties[i].GetGetter().GetClrValueUsingContainingEntity(row);

                if (value is null)
                {
                    await importer.WriteNullAsync(cancellationToken);
                }
                else
                {
                    await importer.WriteAsync(converters[i]?.ConvertToProvider(value) ?? value, properties[i].GetColumnType(), cancellationToken);
                }
            }
        }

        await importer.CompleteAsync(cancellationToken);
    }
}
