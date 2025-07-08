namespace DfE.GIAP.Web.Providers.Session
{
    public interface ISessionProvider
    {
        void SetSessionValue(string key, string value);
        string GetSessionValue(string key);
        void RemoveSessionValue(string key);
        bool ContainsSessionKey(string key);
        void ClearSession();
    }
}
