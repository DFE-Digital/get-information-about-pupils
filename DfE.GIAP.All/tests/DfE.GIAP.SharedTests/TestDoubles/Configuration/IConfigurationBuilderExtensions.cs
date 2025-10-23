using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace DfE.GIAP.SharedTests.TestDoubles.Configuration;
internal static class IConfigurationBuilderExtensions
{
    internal static void AddConfiguration(this IConfigurationBuilder builder, IEnumerable<KeyValuePair<string, string?>> config)
    {
        ArgumentNullException.ThrowIfNull(config);

        builder.Add(
            new MemoryConfigurationSource()
            {
                InitialData = config
            });
    }
}
