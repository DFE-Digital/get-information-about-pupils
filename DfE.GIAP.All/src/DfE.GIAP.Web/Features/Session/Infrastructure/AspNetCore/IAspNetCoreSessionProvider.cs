namespace DfE.GIAP.Web.Features.Session.Infrastructure.AspNetCore;

public interface IAspNetCoreSessionProvider
{
    ISession GetSession();
}
