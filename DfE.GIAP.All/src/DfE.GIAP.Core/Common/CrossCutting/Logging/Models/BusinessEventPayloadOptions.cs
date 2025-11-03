namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

public class BusinessEventPayloadOptions
{
    public BusinessEventCategory EventCategory { get; set; }
    public string EventAction { get; set; } = string.Empty;
    public string EventStatus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object>? Context { get; set; }
}

public enum BusinessEventCategory
{
    UserSignIn,
    Search,
    Download
}
