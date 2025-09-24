namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application;

public interface ILogPayloadEnricher<TPayload, TPayloadOptions>
{
    TPayload Enrich(TPayloadOptions options);
}
