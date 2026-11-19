namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <summary>
/// The maturity of an NPD file, from the <c>U</c>, <c>A</c> or <c>F</c> in its name.
/// </summary>
public enum IngestVersion : short
{
    Unamended = 0,
    Amended = 1,
    Final = 2
}
