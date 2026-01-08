namespace DfE.GIAP.Web.Shared.Session.Infrastructure.AspNetCore;

public interface IAspNetCoreSessionProvider
{
    ISession GetSession();
}
