using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.SharedTests;
using DfE.GIAP.SharedTests.Infrastructure.SearchIndex;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.IntegrationTests;

/// <summary>
/// Abstract base class for integration tests.
/// Implements <see cref="IAsyncLifetime"/> so that xUnit will call
/// <see cref="InitializeAsync"/> before tests run and <see cref="DisposeAsync"/> after.
/// Provides a shared DI container setup and scoped resolution of services.
/// </summary>
[Collection(IntegrationTestCollectionMarker.Name)]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IServiceCollection _serviceDescriptors;
    private IServiceScope? _servicesScope; // Holds the lifetime scope for test services (created once per test class).

    /// <summary>
    /// Constructor initializes the service collection with default test doubles.
    /// </summary>
    protected BaseIntegrationTest()
    {
        _serviceDescriptors = ServiceCollectionTestDoubles.Default();
    }

    /// <summary>
    /// Called by xUnit before any tests run.
    /// Sets up default services, allows derived classes to add their own,
    /// and ensures a scoped service provider is created.
    /// </summary>
    public async Task InitializeAsync()
    {
        SetupSharedTestDependencies();                // Register shared test dependencies
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
    protected TInstanceType ResolveTypeFromScopedContext<TInstanceType>()
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

    /// <summary>
    /// Registers shared test dependencies common to all integration tests.
    /// Derived classes can add more via <see cref="OnInitializeAsync"/>.
    /// </summary>
    private void SetupSharedTestDependencies()
    {
        _serviceDescriptors
            .AddCosmosDbDependencies()
            .AddSharedTestDependencies()
            .ConfigureAzureSearchClients();
    }
}
