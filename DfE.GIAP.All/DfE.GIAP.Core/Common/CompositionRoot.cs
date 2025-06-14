using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Common;
public static class CompositionRoot
{
    public static IServiceCollection AddFeaturesSharedDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddCosmosDbDependencies();
        return services;
    }
}
