using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Common
{
    [ExcludeFromCodeCoverage]
    public class CommonRequestBody
    {
        [JsonProperty("ID")]
        public string Id { get; set; }

        [JsonProperty("TITLE")]
        public string Title { get; set; }

        [JsonProperty("BODY")]
        public string Body { get; set; }

        [JsonProperty("USERACCOUNT")]
        public string UserAccount { get; set; }

        [JsonProperty("USERNAME")]
        public string Username { get; set; }

        [JsonProperty("DOCTYPE")]
        public int DocType { get; set; }

        [JsonProperty("PUBLISHED")]
        public bool Published { get; set; }

        [JsonProperty("ACTION")]
        public int? Action { get; set; }

        [JsonProperty("PINNED")]
        public bool Pinned { get; set; }
    }
}
