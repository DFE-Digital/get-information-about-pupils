using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Gias;

public static class CompositionRoot
{
    public static IServiceCollection AddGiasDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<GiasOptions>()
            .BindConfiguration(GiasOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<GiasExtractClient>((sp, client) =>
            client.BaseAddress = sp.GetRequiredService<IOptions<GiasOptions>>().Value.BaseUrl);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddTransient<GiasDataWriter>();
        services.TryAddTransient<GiasImporter>();

        return services;
    }
}
