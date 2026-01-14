using Newtonsoft.Json;

namespace DfE.GIAP.Web.Shared.Serializer;

#nullable enable
public sealed class NewtonsoftJsonSerializer : IJsonSerializer
{
    public T Deserialize<T>(string json) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        T? res = DeserializeInternal<T>(json);

        if (res is null)
        {
            throw new ArgumentException($"Unable to deserialise to type {typeof(T).Name} input {json}");
        }

        return res;
    }

    public bool TryDeserialize<T>(string json, out T? value) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            value = DeserializeInternal<T>(json);
            return value is not null;
        }
        catch
        {
            value = null;
            return false;
        }
    }

    private static T? DeserializeInternal<T>(string value) => JsonConvert.DeserializeObject<T>(value);

    public string Serialize(object value) => JsonConvert.SerializeObject(value);
}
#nullable restore
