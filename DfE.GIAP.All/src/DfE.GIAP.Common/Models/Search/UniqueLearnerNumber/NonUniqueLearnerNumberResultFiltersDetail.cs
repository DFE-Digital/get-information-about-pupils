using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DfE.GIAP.Domain.Models.Search.UniqueLearnerNumber
{
    [ExcludeFromCodeCoverage]
    public class NonUniqueLearnerNumberResultFiltersDetail
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
