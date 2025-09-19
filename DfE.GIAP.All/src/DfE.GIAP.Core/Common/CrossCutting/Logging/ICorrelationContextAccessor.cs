namespace DfE.GIAP.Core.Common.CrossCutting.Logging;
public interface ICorrelationContextAccessor
{
    string? CorrelationId { get; set; }
    string? UserId { get; set; }
    string? SessionId { get; set; }
}

public class CorrelationContextAccessor : ICorrelationContextAccessor
{
    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
}
