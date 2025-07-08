using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.PrePreparedDownloads
{
    [Serializable()]
    [ExcludeFromCodeCoverage]
    public class PrePreparedDownloads
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Date")]
        public DateTime Date { get; set; }
        [JsonProperty("FileName")]
        public string FileName { get; set; }
        [JsonProperty("Link")]
        public string Link { get; set; }
    }
}
