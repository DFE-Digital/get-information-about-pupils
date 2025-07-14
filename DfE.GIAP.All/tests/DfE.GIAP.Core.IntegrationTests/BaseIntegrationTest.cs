namespace DfE.GIAP.Core.IntegrationTests;
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IServiceCollection _services;
    private IServiceScope? _testServicesScope;
    protected CosmosDbFixture Fixture { get; }

    protected BaseIntegrationTest(CosmosDbFixture fixture)
    {
        _services = ServiceCollectionTestDoubles.Default();
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await SetupAsync();
        await OnInitializeAsync(_services);
        EnsureServicesScope();
    }

    protected virtual Task OnInitializeAsync(IServiceCollection services) => Task.CompletedTask;
    protected virtual Task OnDisposeAsync() => Task.CompletedTask;
    protected T ResolveTypeFromScopedContext<T>() where T : notnull
    {
        EnsureServicesScope();
        return _testServicesScope!.ServiceProvider.GetRequiredService<T>();
    }

    private void EnsureServicesScope()
    {
        ArgumentNullException.ThrowIfNull(_services);
        if (_testServicesScope == null)
        {
            ServiceProvider provider = _services.BuildServiceProvider();
            _testServicesScope = provider.CreateScope();
        }
    }

    private async Task SetupAsync()
    {
        await Fixture.Database.ClearDatabaseAsync();
        _services.AddSharedTestDependencies();
    }

    public async Task DisposeAsync()
    {
        _testServicesScope?.Dispose();
        await OnDisposeAsync();
    }
}
