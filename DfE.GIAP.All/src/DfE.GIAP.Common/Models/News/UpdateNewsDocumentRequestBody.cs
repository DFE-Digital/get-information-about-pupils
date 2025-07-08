using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.News
{
    [ExcludeFromCodeCoverage]
    public class UpdateNewsDocumentRequestBody
    {
        [JsonProperty("ID")]
        public string Id { get; set; }
        [JsonProperty("TITLE")]
        public string Title { get; set; }
        [JsonProperty("BODY")]
        public string Body { get; set; }
        [JsonProperty("DOCTYPE")]
        public int DocType { get; set; }
    }
}
