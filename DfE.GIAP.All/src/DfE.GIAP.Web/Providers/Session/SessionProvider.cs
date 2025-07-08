using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text.Json;

namespace DfE.GIAP.Web.Providers.Session
{
    public class SessionProvider : ISessionProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ??
                throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private ISession Session => _httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("HttpContext or Session is not available. Make sure session middleware is properly configured.");

        public void SetSessionValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "Session key cannot be null or empty.");

            Session.SetString(key, value);
        }

        public string GetSessionValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "Session key cannot be null or empty.");

            return Session.GetString(key);
        }

        public void RemoveSessionValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "Session key cannot be null or empty.");

            Session.Remove(key);
        }

        public bool ContainsSessionKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "Session key cannot be null or empty.");

            return Session.Keys.Contains(key);
        }

        public void ClearSession()
        {
            var keys = Session.Keys.ToList();
            foreach (var key in keys)
            {
                Session.Remove(key);
            }
        }

        public void SetSessionObject<T>(string key, T obj)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var json = JsonSerializer.Serialize(obj);
            Session.SetString(key, json);
        }

        public T GetSessionObject<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var json = Session.GetString(key);
            return json == null ? default : JsonSerializer.Deserialize<T>(json);
        }
    }
}
