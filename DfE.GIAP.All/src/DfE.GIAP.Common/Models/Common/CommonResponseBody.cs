using System;
using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Domain.Models.User;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.Models.Common;

[ExcludeFromCodeCoverage]
public class CommonResponseBody
{
    [JsonProperty("ID")]
    public string Id { get; set; }
    public string Body { get; set; }
    public UserInfo CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime Date { get; set; }
    public string DraftBody { get; set; }
    public string DraftTitle { get; set; }
    public UserInfo ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool Published { get; set; }
    public string Title { get; set; }
    public bool Archived { get; set; }
    public bool Pinned { get; set; }
}
