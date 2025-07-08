using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.News
{
    [ExcludeFromCodeCoverage]
    public class Article
    {
        [JsonProperty("ID")]
        public string Id { get; set; }
        public string Body { get; set; }
        public DateTime Date { get; set; }
        public string DraftBody { get; set; }
        public string DraftTitle { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool Published { get; set; }
        public string Title { get; set; }
        public bool Archived { get; set; }
        public bool Pinned { get; set; }
    }
}
