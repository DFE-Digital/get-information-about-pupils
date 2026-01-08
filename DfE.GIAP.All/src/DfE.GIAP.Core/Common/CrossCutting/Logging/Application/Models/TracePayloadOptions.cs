namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

public class TracePayloadOptions
{
    public LogLevel Level { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
    public string? Category { get; set; }
    public string? Source { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}
