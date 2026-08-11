using DfE.GIAP.SharedTests.Runtime;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;

namespace DfE.GIAP.Core.IntegrationTests.TestHarness;

/// <summary>
/// Abstract base class for integration tests.
/// Implements <see cref="IAsyncLifetime"/> so that xUnit will call
/// <see cref="InitializeAsync"/> before tests run and <see cref="DisposeAsync"/> after.
/// Provides a shared DI container setup and scoped resolution of services.
/// <para>
/// Deliberately carries no <c>[Collection]</c> attribute: tests that need the Cosmos DB emulator
/// opt in with <see cref="CosmosDbIntegrationTestCollectionMarker"/>, so tests that stub everything
/// in-process require no infrastructure.
/// </para>
/// </summary>
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IServiceCollection _serviceDescriptors;
    private readonly GiapCosmosDbFixture? _cosmosDbFixture;
    private IServiceScope? _servicesScope; // Holds the lifetime scope for test services (created once per test class).

    /// <summary>
    /// Constructor initializes the service collection with default test doubles.
    /// </summary>
    /// <param name="cosmosDbFixture">Fixture owning the emulator container. The application under
    /// test is pointed at it, so both the seeding and the application read the same instance.
    /// Omit it for tests that stub every dependency in-process and never open a connection - they
    /// then need no emulator, and must stay out of
    /// <see cref="CosmosDbIntegrationTestCollectionMarker"/>.</param>
    protected BaseIntegrationTest(GiapCosmosDbFixture? cosmosDbFixture = null)
    {
        _cosmosDbFixture = cosmosDbFixture;
        _serviceDescriptors = ServiceCollectionTestDoubles.Default();
    }

    /// <summary>
    /// The emulator container shared by every test in the Cosmos DB integration test collection.
    /// </summary>
    protected GiapCosmosDbFixture CosmosDb =>
        _cosmosDbFixture ?? throw new InvalidOperationException(
            $"This test did not take a {nameof(GiapCosmosDbFixture)}. Apply " +
            $"[Collection({nameof(CosmosDbIntegrationTestCollectionMarker)}.Name)] and pass the " +
            "fixture to the base constructor.");

    /// <summary>
    /// Called by xUnit before any tests run.
    /// Sets up default services, allows derived classes to add their own,
    /// and ensures a scoped service provider is created.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Without a fixture the placeholder Cosmos DB settings are left in place: the registration
        // graph still resolves, but nothing connects.
        if (_cosmosDbFixture is null)
        {
            _serviceDescriptors.AddAspNetCoreRuntimeProvidedServices();
        }
        else
        {
            _serviceDescriptors.AddAspNetCoreRuntimeProvidedServices(
                cosmosDbEndpointUri: _cosmosDbFixture.Connection.Endpoint,
                cosmosDbPrimaryKey: _cosmosDbFixture.Connection.Key);
        }

        _serviceDescriptors.AddFeaturesSharedServices();

        await OnInitializeAsync(_serviceDescriptors); // Allow derived classes to customize

        EnsureServiceScope();  // Build provider and create scope
    }

    /// <summary>
    /// Called by xUnit after all tests have run.
    /// Disposes the service scope and calls the derived class cleanup hook.
    /// </summary>
    public async Task DisposeAsync()
    {
        _servicesScope?.Dispose();
        await OnDisposeAsync();
    }

    /// <summary>
    /// Hook for derived classes to add additional service registrations asynchronously.
    /// Default implementation does nothing.
    /// </summary>
    protected virtual Task OnInitializeAsync(IServiceCollection services) => Task.CompletedTask;

    /// <summary>
    /// Hook for derived classes to perform async cleanup after tests complete.
    /// Default implementation does nothing.
    /// </summary>
    protected virtual Task OnDisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Resolve a service of type <typeparamref name="TInstanceType"/> from the scoped service provider.
    /// Ensures the scope is created before resolving.
    /// </summary>
    protected TInstanceType ResolveApplicationType<TInstanceType>()
        where TInstanceType : notnull
    {
        EnsureServiceScope();
        return _servicesScope!.ServiceProvider.GetRequiredService<TInstanceType>();
    }

    /// <summary>
    /// Ensures that the service provider and scope are created.
    /// If not already created, builds the provider from the service collection
    /// and creates a new scope.
    /// </summary>
    private void EnsureServiceScope()
    {
        ArgumentNullException.ThrowIfNull(_serviceDescriptors);

        if (_servicesScope == null)
        {
            ServiceProvider provider = _serviceDescriptors.BuildServiceProvider();
            _servicesScope = provider.CreateScope();
        }
    }
}
