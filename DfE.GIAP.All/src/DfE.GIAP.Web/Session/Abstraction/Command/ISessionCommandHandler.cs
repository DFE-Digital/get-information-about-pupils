namespace DfE.GIAP.Web.Session.Abstraction.Command;

public interface ISessionCommandHandler<in TValue>
{
    void StoreInSession(TValue value);
}
