﻿using DfE.GIAP.Web.Session.Abstraction;
using DfE.GIAP.Web.Session.Infrastructure;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Web.Session;

public static class CompositionRoot
{
    public static IServiceCollection AddAspNetCoreSessionServices(this IServiceCollection services)
    {
        services.TryAddScoped<IAspNetCoreSessionProvider, AspNetCoreSessionProvider>();
        services.TryAddSingleton<ISessionObjectKeyResolver, SessionObjectKeyResolver>();
        return services;
    }
}
