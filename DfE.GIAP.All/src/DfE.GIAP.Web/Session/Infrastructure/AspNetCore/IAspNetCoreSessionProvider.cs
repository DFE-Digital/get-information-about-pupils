namespace DfE.GIAP.Web.Session.Infrastructure.AspNetCore;

public interface IAspNetCoreSessionProvider
{
    ISession GetSession();
}
