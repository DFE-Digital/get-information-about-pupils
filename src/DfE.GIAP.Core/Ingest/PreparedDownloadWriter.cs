using System.Data.Common;
using System.Text;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace DfE.GIAP.Core.Ingest;

/// <summary>
/// One kind of prepared download, e.g. the pupil premium files for LAs: a CSV file for each organisation, in its own
/// folder.
/// </summary>
/// <param name="Name">Used in log messages, e.g. <c>LA</c>.</param>
/// <param name="FolderName">Each organisation's file is written to <c>{FolderName}/{key}/</c>.</param>
/// <param name="FileName">The file's name without <c>.csv</c>. A <c>*</c> is replaced by the file's first row's
/// <see cref="PreparedDownloadRow.FileNamePart"/>, or by the group's UID if <paramref name="GroupTypeCode"/> is set.</param>
/// <param name="Header">The column names; for a group, including the group's UID.</param>
/// <param name="GroupTypeCode">If set, each row's key is a URN, and the row is written to the file of every open GIAS
/// group of this type that the establishment is linked to (see <see cref="EstablishmentGroup"/>). The group's UID is
/// the file's key and the first value in each row.</param>
public sealed record PreparedDownloadAudience(
    string Name,
    string FolderName,
    string FileName,
    IReadOnlyList<string> Header,
    int? GroupTypeCode = null);

/// <param name="Audience">The index of the row's <see cref="PreparedDownloadAudience"/>.</param>
/// <param name="Key">The organisation whose file the row is written to, e.g. the LA. A row with no key isn't written.</param>
/// <param name="FileNamePart">Replaces the <c>*</c> in <see cref="PreparedDownloadAudience.FileName"/>; the file's
/// first row's is used, and a file whose first row has none isn't written.</param>
/// <param name="RowNumber">The row's position in the data file; each file's rows are written in this order.</param>
public sealed record PreparedDownloadRow(
    int Audience,
    string? Key,
    string? FileNamePart,
    int RowNumber,
    IReadOnlyList<string?> Values);

/// <summary>
/// Writes prepared downloads from the rows of a data file, replacing any files with the same names.
/// </summary>
/// <remarks>
/// The rows are streamed into a temporary table and grouped into files in Postgres, so a data file is never held in
/// memory. Files are UTF-8 without a BOM, with <c>\n</c> line endings; values are only quoted if they contain a comma,
/// quote or line break, and a null value is empty.
/// </remarks>
public class PreparedDownloadWriter(
    IDbContextFactory<GiapDbContext> dbContextFactory,
    PreparedDownloadPublisher publisher,
    ILogger<PreparedDownloadWriter> logger)
{
    private const int MaxConcurrentUploads = 16;

    private static readonly UTF8Encoding _encoding = new(encoderShouldEmitUTF8Identifier: false);

    /// <returns>The number of files written.</returns>
    public async Task<int> WriteAsync(
        IReadOnlyList<PreparedDownloadAudience> audiences,
        IAsyncEnumerable<PreparedDownloadRow> rows,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(audiences);
        ArgumentNullException.ThrowIfNull(rows);

        await using GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TEMP TABLE prepared_download_rows (
                audience integer NOT NULL,
                key text NOT NULL,
                file_name_part text,
                row_number integer NOT NULL,
                line text NOT NULL
            ) ON COMMIT DROP
            """,
            cancellationToken);

        NpgsqlConnection connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();

        int[] rowsWithoutKey = await CopyRowsAsync(connection, audiences.Count, rows, cancellationToken);

        // Temp tables aren't analyzed automatically, and the planner needs their statistics for the joins below
        await dbContext.Database.ExecuteSqlRawAsync("ANALYZE prepared_download_rows", cancellationToken);

        int filesWritten = 0;

        for (int i = 0; i < audiences.Count; i++)
        {
            if (rowsWithoutKey[i] > 0)
            {
                logger.LogWarning(
                    "{RowCount} rows weren't written to the {Audience} prepared downloads because they have no key.",
                    rowsWithoutKey[i],
                    audiences[i].Name);
            }

            filesWritten += await WriteAudienceAsync(connection, i, audiences[i], cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return filesWritten;
    }

    /// <summary>
    /// Formats values as a CSV line, without a line ending.
    /// </summary>
    public static string FormatLine(IEnumerable<string?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        return string.Join(',', values.Select(FormatValue));
    }

    private static string FormatValue(string? value) =>
        value is null ? "" :
        value.AsSpan().IndexOfAny(",\"\r\n") >= 0 ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"" :
        value;

    // Returns the number of rows with no key for each audience; they aren't copied
    private static async Task<int[]> CopyRowsAsync(
        NpgsqlConnection connection,
        int audienceCount,
        IAsyncEnumerable<PreparedDownloadRow> rows,
        CancellationToken cancellationToken)
    {
        int[] rowsWithoutKey = new int[audienceCount];

        await using NpgsqlBinaryImporter importer = await connection.BeginBinaryImportAsync(
            "COPY prepared_download_rows (audience, key, file_name_part, row_number, line) FROM STDIN (FORMAT BINARY)",
            cancellationToken);

        await foreach (PreparedDownloadRow row in rows.WithCancellation(cancellationToken))
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row.Audience, audienceCount, nameof(rows));

            if (row.Key is null)
            {
                rowsWithoutKey[row.Audience]++;
                continue;
            }

            await importer.StartRowAsync(cancellationToken);
            await importer.WriteAsync(row.Audience, NpgsqlDbType.Integer, cancellationToken);
            await importer.WriteAsync(row.Key, NpgsqlDbType.Text, cancellationToken);

            if (row.FileNamePart is null)
            {
                await importer.WriteNullAsync(cancellationToken);
            }
            else
            {
                await importer.WriteAsync(row.FileNamePart, NpgsqlDbType.Text, cancellationToken);
            }

            await importer.WriteAsync(row.RowNumber, NpgsqlDbType.Integer, cancellationToken);
            await importer.WriteAsync(FormatLine(row.Values), NpgsqlDbType.Text, cancellationToken);
        }

        await importer.CompleteAsync(cancellationToken);

        return rowsWithoutKey;
    }

    private async Task<int> WriteAudienceAsync(
        NpgsqlConnection connection,
        int audienceIndex,
        PreparedDownloadAudience audience,
        CancellationToken cancellationToken)
    {
        // The rows come back grouped into files, in file order
        await using NpgsqlCommand command = audience.GroupTypeCode is { } groupTypeCode
            ? new NpgsqlCommand(
                """
                SELECT l.group_uid::text, l.group_uid::text, l.group_uid::text || ',' || r.line
                FROM prepared_download_rows r
                JOIN establishment_group_links l ON l.urn::text = r.key
                JOIN establishment_groups g ON g.group_uid = l.group_uid
                WHERE r.audience = $1 AND g.group_type_code = $2 AND g.group_status_code = $3
                ORDER BY l.group_uid, r.row_number
                """,
                connection)
            {
                Parameters = { new() { Value = audienceIndex }, new() { Value = groupTypeCode }, new() { Value = EstablishmentGroup.OpenGroupStatusCode } }
            }
            : new NpgsqlCommand(
                """
                SELECT key, file_name_part, line
                FROM prepared_download_rows
                WHERE audience = $1
                ORDER BY key, row_number
                """,
                connection)
            {
                Parameters = { new() { Value = audienceIndex } }
            };

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        string header = FormatLine(audience.Header);
        using SemaphoreSlim uploadSlots = new(MaxConcurrentUploads);
        List<Task> uploads = [];
        int filesWithoutName = 0;

        string? key = null;
        MemoryStream? content = null;
        StreamWriter? writer = null;
        string? blobName = null;

        try
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                string rowKey = reader.GetString(0);

                if (rowKey != key)
                {
                    await CompleteFileAsync();

                    key = rowKey;
                    string? fileNamePart = await reader.IsDBNullAsync(1, cancellationToken) ? null : reader.GetString(1);

                    if (fileNamePart is null)
                    {
                        filesWithoutName++;
                        continue;
                    }

                    blobName = $"{audience.FolderName}/{key}/{audience.FileName.Replace("*", fileNamePart, StringComparison.Ordinal)}.csv";
                    content = new MemoryStream();
                    writer = new StreamWriter(content, _encoding, leaveOpen: true) { NewLine = "\n" };
                    await writer.WriteLineAsync(header);
                }

                if (writer is not null)
                {
                    await writer.WriteLineAsync(reader.GetString(2));
                }
            }

            await CompleteFileAsync();
        }
        finally
        {
            // Lets any uploads that have started finish before the exception is thrown
            await Task.WhenAll(uploads).ContinueWith(_ => { }, TaskScheduler.Default);
        }

        await Task.WhenAll(uploads);

        if (filesWithoutName > 0)
        {
            logger.LogWarning(
                "{FileCount} {Audience} prepared downloads weren't written because their first row has nothing to name them by.",
                filesWithoutName,
                audience.Name);
        }

        logger.LogInformation("Wrote {FileCount} {Audience} prepared downloads.", uploads.Count, audience.Name);

        return uploads.Count;

        async Task CompleteFileAsync()
        {
            if (writer is null)
            {
                return;
            }

            await writer.DisposeAsync();
            writer = null;

            MemoryStream fileContent = content!;
            string fileBlobName = blobName!;
            fileContent.Position = 0;

            // Bounds how many files are held in memory, as well as how many are uploaded at once
            await uploadSlots.WaitAsync(cancellationToken);
            uploads.Add(UploadAsync());

            async Task UploadAsync()
            {
                try
                {
                    await using (fileContent)
                    {
                        await publisher.PublishAsync(fileBlobName, fileContent, cancellationToken);
                    }
                }
                finally
                {
                    uploadSlots.Release();
                }
            }
        }
    }
}
