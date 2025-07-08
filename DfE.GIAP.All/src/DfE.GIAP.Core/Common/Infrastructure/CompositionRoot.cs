using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Common.Infrastructure;
internal static class CompositionRoot
{
    internal static IServiceCollection AddTemporaryCosmosClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<CosmosClient>(sp =>
        {
            IOptions<RepositoryOptions> repositoryOptions = sp.GetRequiredService<IOptions<RepositoryOptions>>();
            RepositoryOptions options = repositoryOptions.Value ?? throw new ArgumentException($"{repositoryOptions.Value} is null");

            ArgumentException.ThrowIfNullOrWhiteSpace(options.EndpointUri);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.PrimaryKey);

            CosmosClientBuilder cosmosClientBuilder = new(options.EndpointUri, options.PrimaryKey);

            // Check if the environment is local to use Gateway
            string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (!string.IsNullOrWhiteSpace(environment) && environment.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                cosmosClientBuilder.WithConnectionModeGateway(); // Use gateway for local running only
            }

            return cosmosClientBuilder.Build();
        });

        return services;
    }
}
