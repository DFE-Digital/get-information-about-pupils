using DfE.GIAP.Common.Constants.AzureFunction;
using Microsoft.AspNetCore.Http;

namespace DfE.GIAP.Web.Tests.TestDoubles;

public class SessionFake : ISession
{

    public SessionFake()
    {
        Values = [];
    }

    public string Id => HeaderDetails.SessionId;

    public bool IsAvailable => true;

    public IEnumerable<string> Keys => Values.Keys;

    public Dictionary<string, byte[]> Values { get; set; }

    public void Clear() => Values.Clear();

    public Task CommitAsync(CancellationToken token) => throw new NotImplementedException();

    public Task LoadAsync(CancellationToken token) => throw new NotImplementedException();

    public void Remove(string key) => Values.Remove(key);

    public void Set(string key, byte[] value)
    {
        if (Values.ContainsKey(key))
        {
            Remove(key);
        }
        Values.Add(key, value);
    }

    public bool TryGetValue(string key, out byte[] value)
    {
        if (Values.TryGetValue(key, out byte[]? output))
        {
            value = output;
            return true;
        }
        value = [];
        return false;
    }
}
