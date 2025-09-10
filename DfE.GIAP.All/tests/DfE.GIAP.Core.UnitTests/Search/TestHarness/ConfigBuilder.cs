using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestHarness;

/// <summary>
/// Provides a test harness for constructing <see cref="IConfiguration"/> instances
/// using in-memory key-value pairs. Useful for simulating configuration settings
/// in unit tests without relying on external files or environment variables.
/// </summary>
public sealed class ConfigBuilder
{
    /// <summary>
    /// Builds an <see cref="IConfiguration"/> object using the provided dictionary of options.
    /// Enables injection of test-specific configuration values for adapter and service testing.
    /// </summary>
    /// <param name="options">A dictionary of configuration keys and values to simulate app settings.</param>
    /// <returns>A fully constructed <see cref="IConfiguration"/> instance backed by in-memory data.</returns>
    public IConfiguration SetupConfiguration(Dictionary<string, string?> options)
    {
        ConfigurationBuilder configBuilder = new();

        // Inject the provided key-value pairs into the configuration pipeline
        configBuilder.AddInMemoryCollection(options);

        // Finalize and return the built configuration object
        return configBuilder.Build();
    }
}
