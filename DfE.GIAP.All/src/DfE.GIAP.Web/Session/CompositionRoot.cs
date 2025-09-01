using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Session.Abstraction;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore.Query;
using DfE.GIAP.Web.Session.Infrastructure;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore.Command;
using DfE.GIAP.Web.Session.Infrastructure.Serialization;

namespace DfE.GIAP.Web.Session;

public static class CompositionRoot
{
    public static IServiceCollection AddAspNetCoreSessionServices(this IServiceCollection services)
    {
        services.TryAddScoped<IAspNetCoreSessionProvider, AspNetCoreSessionProvider>();

        services.TryAddScoped(typeof(ISessionQueryHandler<>), typeof(AspNetCoreSessionQueryHandler<>));
        services.TryAddScoped(typeof(ISessionCommandHandler<>), typeof(AspNetCoreSessionCommandHandler<>));

        services.TryAddSingleton<ISessionObjectKeyResolver, SessionObjectKeyResolver>();
        services.TryAddSingleton(typeof(ISessionObjectSerializer<>), typeof(DefaultSessionObjectSerializer<>));
        return services;
    }
}
