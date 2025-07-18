namespace DfE.GIAP.Web.Providers.Cookie;

public class CookieProvider : ICookieProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieProvider(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext HttpContext =>
        _httpContextAccessor.HttpContext ??
        throw new InvalidOperationException("HttpContext is not available.");

    private IRequestCookieCollection RequestCookies =>
        HttpContext.Request.Cookies;

    private IResponseCookies ResponseCookies =>
        HttpContext.Response.Cookies;

    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key), "Cookie key cannot be null or empty.");

        string storedCookieValue = RequestCookies[key];
        return storedCookieValue is null ? null : Uri.UnescapeDataString(storedCookieValue);
    }

    public bool Contains(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key), "Cookie key cannot be null or empty.");

        return RequestCookies.ContainsKey(key);
    }

    public void Delete(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key), "Cookie key cannot be null or empty.");

        ResponseCookies.Delete(key);
    }

    public void Set(string key, string value, bool isEssential = false, int? expireTime = 20, CookieOptions option = null)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(key), "Cookie key and value cannot be null or empty.");

        if (expireTime == 1)
            expireTime = 20;

        if (option == null)
        {
            option = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(expireTime.Value)
            };
        }

        option.Secure = true;
        option.HttpOnly = true;

        if (isEssential)
            option.IsEssential = true;

        ResponseCookies.Append(key, value, option);
    }

    public void ClearCookies()
    {
        foreach (string key in RequestCookies.Keys.ToList())
        {
            ResponseCookies.Delete(key);
        }
    }
}
