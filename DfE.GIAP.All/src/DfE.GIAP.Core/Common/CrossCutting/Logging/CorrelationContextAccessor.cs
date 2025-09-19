namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public class CorrelationContextAccessor : ICorrelationContextAccessor
{
    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
}
