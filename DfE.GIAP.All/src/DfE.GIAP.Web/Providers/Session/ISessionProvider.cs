namespace DfE.GIAP.Web.Providers.Session;
#nullable enable
public interface ISessionProvider
{
    void SetSessionValue(string key, string value);
    void SetSessionValue<T>(string key, T value);
    string? GetSessionValue(string key);
    T? GetSessionValueOrDefault<T>(string key);
    void RemoveSessionValue(string key);
    bool ContainsSessionKey(string key);
    void ClearSession();
}
