using DfE.GIAP.Web.Features.Session.Command;
using DfE.GIAP.Web.Features.Session.Infrastructure.KeyResolver;
using DfE.GIAP.Web.Features.Session.Infrastructure.Provider;
using DfE.GIAP.Web.Features.Session.Infrastructure.Serialization;
using DfE.GIAP.Web.Features.Session.Query;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Web.Features.Session;

public static class CompositionRoot
{
    public static IServiceCollection AddSessionServices(this IServiceCollection services)
    {
        services.TryAddScoped<IAspNetCoreSessionProvider, AspNetCoreSessionProvider>();

        services.TryAddScoped(typeof(ISessionQueryHandler<>), typeof(AspNetCoreSessionQueryHandler<>));
        services.TryAddScoped(typeof(ISessionCommandHandler<>), typeof(AspNetCoreSessionCommandHandler<>));

        services.TryAddSingleton<ISessionObjectKeyResolver, SessionObjectKeyResolver>();
        services.TryAddSingleton(typeof(ISessionObjectSerializer<>), typeof(DefaultSessionObjectSerializer<>));
        return services;
    }
}
