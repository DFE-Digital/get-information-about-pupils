namespace DfE.GIAP.Core.Common.CrossCutting.Logging;
public interface ICorrelationContextAccessor
{
    string? CorrelationId { get; set; }
    string? UserId { get; set; }
    string? SessionId { get; set; }
}
