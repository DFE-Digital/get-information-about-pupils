using Microsoft.AspNetCore.Http;

namespace DfE.GIAP.Common.Helpers.CookieManager
{
    public interface ICookieManager
    {
        string Get(string key);

        void Set(string key, string value, bool isEssential = false, int ? expireTime = 1, CookieOptions option = null);

        bool Contains(string key);

        void Delete(string key);
    }
}
