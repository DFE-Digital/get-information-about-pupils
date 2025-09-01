using DfE.GIAP.Web.Features.Session.Abstractions;
using DfE.GIAP.Web.Features.Session.Abstractions.Command;
using DfE.GIAP.Web.Features.Session.Abstractions.Query;
using DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore;
using DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore.Command;
using DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore.Query;
using DfE.GIAP.Web.Features.Session.Infrastructure.KeyResolver;
using DfE.GIAP.Web.Features.Session.Infrastructure.Serialization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Web.Features.Session;

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
