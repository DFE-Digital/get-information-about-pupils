namespace DfE.GIAP.Core.Logging.Application;

public interface ILogPayloadBuilder<TPayload>
{
    TPayload BuildPayload(ILogPayloadOptions options);
}
