using Microsoft.AspNetCore.Http;
using System;

namespace DfE.GIAP.Common.Helpers.CookieManager
{
    public class CookieManager : ICookieManager
    {
        private readonly HttpContext _httpContext;

        public CookieManager(IHttpContextAccessor httpAccessor)
        {
            _httpContext = httpAccessor.HttpContext;
        }

        public string Get(string key)
        {
            var storedCookieValue = _httpContext.Request.Cookies[key];

            if (storedCookieValue == null) return null;

            return Uri.UnescapeDataString(storedCookieValue);
        }

        public bool Contains(string key)
        {
            if (_httpContext == null)
            {
                throw new ArgumentNullException(nameof(_httpContext));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _httpContext.Request.Cookies.ContainsKey(key);
        }

        public void Delete(string key)
        {
            _httpContext.Response.Cookies.Delete(key);
        }

        /// <summary>
        /// Set Cookie
        /// </summary>
        /// <param name="key">Cookie Name</param>
        /// <param name="value">Cookie Value</param>
        /// <param name="expireTime">Expire time in minutes (default time is 1 minute)</param>
        /// <param name="option">Cookie options - optional  </param>
        public void Set(string key, string value, bool isEssential = false, int? expireTime = 20, CookieOptions option = null)
        {
            if (expireTime == 1)
            {
                expireTime = 20;
            }

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(key));
            }

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
            {
                option.IsEssential = true;
            }

            _httpContext.Response.Cookies.Append(key, value, option);
        }
    }
}
