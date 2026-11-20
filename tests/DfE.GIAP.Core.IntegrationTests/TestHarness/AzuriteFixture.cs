using DfE.GIAP.SharedTests.Infrastructure.Azurite;

namespace DfE.GIAP.Core.IntegrationTests.TestHarness;

public sealed class AzuriteFixture : IAsyncLifetime
{
    private readonly AzuriteServer _server = new();

    /// <summary>
    /// Only valid once <see cref="InitializeAsync"/> has completed.
    /// </summary>
    public string ConnectionString => _server.ConnectionString;

    public Task InitializeAsync() => _server.StartAsync();

    public async Task DisposeAsync() => await _server.DisposeAsync();
}
