using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Features.Search.LegacyModels;

[ExcludeFromCodeCoverage]
public class NonUpnResultsList : IRbac
{
    [JsonProperty("@search.score")]
    public decimal Score { get; set; }
    [JsonProperty("UPN")]
    public string LearnerNumber { get; set; }
    public string LearnerNumberId { get; set; }
    public string Surname { get; set; }
    public string Forename { get; set; }
    public char? Gender { get; set; }
    [JsonProperty("DOB")]
    public DateTime? DOB { get; set; }
}
