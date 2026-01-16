namespace DfE.GIAP.Web.Shared.Serializer;
#nullable enable
public interface IJsonSerializer
{
    string Serialize(object value);
    T Deserialize<T>(string json) where T : class;
    bool TryDeserialize<T>(string json, out T? value) where T : class;
}
