using System.Text.Json;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.Session.Infrastructure.KeyResolver;
using DfE.GIAP.Web.Features.Session.Infrastructure.Provider;

namespace DfE.GIAP.Web.Features.Session.Command;

public sealed class AspNetCoreSessionCommandHandler<TValue> : ISessionCommandHandler<TValue>
{
    private readonly IAspNetCoreSessionProvider _sessionProvider;
    private readonly ISessionObjectKeyResolver _sessionKeyResolver;

    public AspNetCoreSessionCommandHandler(
        IAspNetCoreSessionProvider sessionProvider,
        ISessionObjectKeyResolver sessionKeyResolver)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(sessionKeyResolver);
        _sessionKeyResolver = sessionKeyResolver;
    }

    private string SessionObjectAccessKey => _sessionKeyResolver.Resolve(typeof(TValue));

    public void StoreInSession(TValue value) => SetSessionValue(value);

    public void StoreInSession<TDataTransferObject>(
        TValue value,
        IMapper<TValue, TDataTransferObject> mapSessionObjectToDto)
    {
        TDataTransferObject dto = mapSessionObjectToDto.Map(value);
        SetSessionValue(dto);
    }

    private void SetSessionValue<TStore>(TStore setValue)
    {
        ArgumentNullException.ThrowIfNull(setValue);

        string json = JsonSerializer.Serialize(setValue);

        _sessionProvider
            .GetSession()
            .SetString(SessionObjectAccessKey, json);
    }
}
