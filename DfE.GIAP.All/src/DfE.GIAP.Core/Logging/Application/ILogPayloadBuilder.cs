namespace DfE.GIAP.Core.Logging.Application;

public interface ILogPayloadBuilder<TPayload, TPayloadOptions>
{
    TPayload Build(TPayloadOptions options);
}
