namespace DfE.GIAP.SharedTests.Infrastructure.CosmosDb;

/// <summary>
/// Connection details for an emulator instance, handed to the application under test
/// so it targets the same container the fixture seeds.
/// </summary>
public sealed record CosmosDbConnection(string Endpoint, string Key)
{
    /// <summary>
    /// Placeholder used before the emulator container has started, so that reading the
    /// connection too early fails with a clear message rather than a null reference.
    /// </summary>
    public static CosmosDbConnection NotStarted { get; } = new(string.Empty, string.Empty);
}
