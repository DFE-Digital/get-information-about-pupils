using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;

public class RawCosmosDocument
{
    [JsonExtensionData]
    public Dictionary<string, JToken>? Fields { get; set; }
}
