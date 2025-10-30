namespace DfE.GIAP.SharedTests.Infrastructure.WireMock;
public interface IWireMockClient : IDisposable
{
    Task Stub<TDataTransferObject>(
        RequestMatch request,
        Response<TDataTransferObject> response);
}
