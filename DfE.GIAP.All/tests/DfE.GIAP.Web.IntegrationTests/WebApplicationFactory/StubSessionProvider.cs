using System.Text.Json;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.IntegrationTests.WebApplicationFactory;
public class StubSessionProvider : ISessionProvider
{
    private readonly Dictionary<string, string> _store = [];

    public void SetSessionValue(string key, string value) => _store[key] = value;

    public void SetSessionValue<T>(string key, T value) => _store[key] = JsonSerializer.Serialize(value);

    public string? GetSessionValue(string key) => _store.TryGetValue(key, out string? value) ? value : null;

    public T? GetSessionValueOrDefault<T>(string key) =>
        _store.TryGetValue(key, out string? value) ?
            JsonSerializer.Deserialize<T>(value) :
                default;

    public void RemoveSessionValue(string key) => _store.Remove(key);

    public bool ContainsSessionKey(string key) => _store.ContainsKey(key);

    public void ClearSession() => _store.Clear();
}
