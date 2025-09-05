using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace DfE.GIAP.Common.Models.Common;

[ExcludeFromCodeCoverage]
public class CommonResponseBody
{
    [JsonProperty("ID")]
    public string Id { get; set; }
    public string Body { get; set; }
    public string Title { get; set; }
}
