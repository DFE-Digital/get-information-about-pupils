namespace DfE.GIAP.Web.Session.Abstraction.Command;

public interface ISessionCommandHandler<in TValue> where TValue : class
{
    void StoreInSession(TValue value);
}
