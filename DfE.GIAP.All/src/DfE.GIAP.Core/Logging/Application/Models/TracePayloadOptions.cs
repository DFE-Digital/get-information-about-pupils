using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Logging.Application.Models;

public class TracePayloadOptions : ILogPayloadOptions
{
    public LogLevel Level { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
    public string? Category { get; set; }
    public string? Source { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}

public class AuditPayloadOptions : ILogPayloadOptions
{
    public string? Message { get; set; }
    public string? Action { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}

public class ErrorPayloadOptions : ILogPayloadOptions
{
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
    public string? Source { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}
