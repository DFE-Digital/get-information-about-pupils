using DotNet.Testcontainers.Builders;
using Testcontainers.CosmosDb;

namespace DfE.GIAP.SharedTests.Infrastructure.CosmosDb;

/// <summary>
/// Owns the lifetime of a Cosmos DB emulator container, started on demand via Testcontainers.
/// The container publishes the gateway on an ephemeral host port, so <see cref="Endpoint"/>
/// is only valid once <see cref="StartAsync"/> has completed.
/// </summary>
/// <remarks>
/// The vNext (Linux) emulator serves plain HTTP by default and only supports gateway
/// connection mode, which is why no certificate handling is required here.
/// </remarks>
public sealed class CosmosDbEmulator : IAsyncDisposable
{
    // The vNext emulator is the only image published for both x64 and arm64.
    private const string ImageName = "mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-latest";
    private const ushort HealthProbePort = 8080;

    private readonly CosmosDbContainer _container;

    public CosmosDbEmulator()
    {
        _container = new CosmosDbBuilder(ImageName)
            .WithPortBinding(HealthProbePort, assignRandomHostPort: true)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(
                        request => request.ForPort(HealthProbePort).ForPath("/ready")))
            .Build();
    }

    /// <summary>
    /// The account key the emulator accepts. Well-known and identical for every emulator instance.
    /// </summary>
    public string Key => CosmosDbBuilder.DefaultAccountKey;

    /// <summary>
    /// The gateway endpoint of the running emulator, including the mapped host port.
    /// </summary>
    public string Endpoint => _container.GetAccountEndpoint();

    public Task StartAsync() => _container.StartAsync();

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
