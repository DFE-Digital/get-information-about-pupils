using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;
using DfE.GIAP.SharedTests;

namespace DfE.GIAP.Core.IntegrationTests;

/// <summary>
/// Abstract base class for integration tests.
/// Implements <see cref="IAsyncLifetime"/> so that xUnit will call
/// <see cref="InitializeAsync"/> before tests run and <see cref="DisposeAsync"/> after.
/// Provides a shared DI container setup and scoped resolution of services.
/// </summary>
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    // Holds the lifetime scope for test services (created once per test class).
    private IServiceScope? _testServicesScope;

    /// <summary>
    /// The service collection used to register dependencies for the test.
    /// Derived classes can add or override registrations before the scope is built.
    /// </summary>
    public IServiceCollection serviceDescriptors { get; }

    /// <summary>
    /// Constructor initializes the service collection with default test doubles.
    /// </summary>
    protected BaseIntegrationTest()
    {
        serviceDescriptors = ServiceCollectionTestDoubles.Default();
    }

    /// <summary>
    /// Called by xUnit before any tests run.
    /// Sets up default services, allows derived classes to add their own,
    /// and ensures a scoped service provider is created.
    /// </summary>
    public async Task InitializeAsync()
    {
        Setup();                // Register shared test dependencies
        await OnInitializeAsync(serviceDescriptors); // Allow derived classes to customize
        EnsureServicesScope();  // Build provider and create scope
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
    protected TInstanceType ResolveTypeFromScopedContext<TInstanceType>()
        where TInstanceType : notnull
    {
        EnsureServicesScope();
        return _testServicesScope!.ServiceProvider.GetRequiredService<TInstanceType>();
    }

    /// <summary>
    /// Ensures that the service provider and scope are created.
    /// If not already created, builds the provider from the service collection
    /// and creates a new scope.
    /// </summary>
    private void EnsureServicesScope()
    {
        ArgumentNullException.ThrowIfNull(serviceDescriptors);

        if (_testServicesScope == null)
        {
            ServiceProvider provider = serviceDescriptors.BuildServiceProvider();
            _testServicesScope = provider.CreateScope();
        }
    }

    /// <summary>
    /// Registers shared test dependencies common to all integration tests.
    /// Derived classes can add more via <see cref="OnInitializeAsync"/>.
    /// </summary>
    private void Setup()
    {
        serviceDescriptors
            .AddCosmosDbDependencies()
            .AddSharedTestDependencies()
            .ConfigureAzureSearchClients();
    }

    /// <summary>
    /// Called by xUnit after all tests have run.
    /// Disposes the service scope and calls the derived class cleanup hook.
    /// </summary>
    public async Task DisposeAsync()
    {
        _testServicesScope?.Dispose();
        await OnDisposeAsync();
    }
}
