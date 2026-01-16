namespace DfE.GIAP.Web.Shared.Session.Infrastructure;

public interface IAspNetCoreSessionProvider
{
    ISession GetSession();
}
