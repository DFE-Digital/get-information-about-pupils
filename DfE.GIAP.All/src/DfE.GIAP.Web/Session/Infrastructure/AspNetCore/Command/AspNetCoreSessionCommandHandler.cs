﻿using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Session.Abstraction;

namespace DfE.GIAP.Web.Session.Infrastructure.AspNetCore.Command;

public sealed class AspNetCoreSessionCommandHandler<TValue> : ISessionCommandHandler<TValue> where TValue : class
{
    private readonly IAspNetCoreSessionProvider _sessionProvider;
    private readonly ISessionObjectKeyResolver _sessionKeyResolver;
    private readonly ISessionObjectSerializer<TValue> _sessionObjectSerializer;

    public AspNetCoreSessionCommandHandler(
        IAspNetCoreSessionProvider sessionProvider,
        ISessionObjectKeyResolver sessionKeyResolver,
        ISessionObjectSerializer<TValue> sessionObjectSerializer)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(sessionKeyResolver);
        _sessionKeyResolver = sessionKeyResolver;

        ArgumentNullException.ThrowIfNull(sessionObjectSerializer);
        _sessionObjectSerializer = sessionObjectSerializer;
    }

    public void StoreInSession(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        string json = _sessionObjectSerializer.Serialize(value);

        _sessionProvider
            .GetSession()
            .SetString(key: _sessionKeyResolver.Resolve<TValue>(), json);
    }
}
