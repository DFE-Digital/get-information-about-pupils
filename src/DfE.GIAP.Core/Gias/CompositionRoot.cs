using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.Gias;

public static class CompositionRoot
{
    public static IServiceCollection AddGiasDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient<GiasExtractClient>(client => client.BaseAddress = new Uri(GiasExtractClient.BaseUrl));

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddTransient<GiasDataWriter>();
        services.TryAddTransient<GiasImporter>();

        return services;
    }
}
