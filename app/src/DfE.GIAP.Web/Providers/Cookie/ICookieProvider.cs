namespace DfE.GIAP.Web.Providers.Cookie;

public interface ICookieProvider
{
    void ClearCookies();
    bool Contains(string key);
    void Delete(string key);
    string Get(string key);
    void Set(string key, string value, bool isEssential = false, int? expireTime = 20, CookieOptions option = null);
}
