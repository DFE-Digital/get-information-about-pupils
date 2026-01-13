using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Web.Shared.Session;

public static class CompositionRoot
{
    public static IServiceCollection AddAspNetCoreSessionServices(this IServiceCollection services)
    {
        services.TryAddScoped<IAspNetCoreSessionProvider, AspNetCoreSessionProvider>();
        services.TryAddSingleton<ISessionObjectKeyResolver, SessionObjectKeyResolver>();
        return services;
    }
}
