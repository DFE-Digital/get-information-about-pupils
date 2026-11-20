using Testcontainers.Azurite;

namespace DfE.GIAP.SharedTests.Infrastructure.Azurite;

/// <summary>
/// Owns the lifetime of an Azurite (Azure Storage emulator) container, started on demand via Testcontainers.
/// <see cref="ConnectionString"/> is only valid once <see cref="StartAsync"/> has completed.
/// </summary>
public sealed class AzuriteServer : IAsyncDisposable
{
    private readonly AzuriteContainer _container;

    public AzuriteServer()
    {
        _container = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.35.0").Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public Task StartAsync() => _container.StartAsync();

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
