namespace DfE.GIAP.Web.Features.Session.Infrastructure.Provider;

public interface IAspNetCoreSessionProvider
{
    ISession GetSession();
}
