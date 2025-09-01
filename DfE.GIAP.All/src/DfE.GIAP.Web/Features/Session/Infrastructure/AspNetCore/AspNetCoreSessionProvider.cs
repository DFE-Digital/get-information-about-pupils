namespace DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore;

public sealed class AspNetCoreSessionProvider : IAspNetCoreSessionProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AspNetCoreSessionProvider(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;
    }

    public ISession GetSession()
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException("HttpContext is not available. Unable to retrieve session");
        }

        if (_httpContextAccessor.HttpContext.Session is null)
        {
            throw new InvalidOperationException("Session is not available. Make sure session middleware is properly configured.");
        }

        return _httpContextAccessor.HttpContext.Session;
    }
}
