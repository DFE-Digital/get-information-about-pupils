namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <summary>
/// A row of a data file that wasn't written, and why.
/// </summary>
public class IngestRejectedRow
{
    public required Guid RunId { get; init; }

    /// <summary>
    /// The row's 1-based position in the data file, not counting the header.
    /// </summary>
    public required int RowNumber { get; init; }

    /// <summary>
    /// The row as a JSON object of column name to value.
    /// </summary>
    public required string Data { get; init; }

    public required string[] Reasons { get; init; }
}
