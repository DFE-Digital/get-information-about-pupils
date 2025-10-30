namespace DfE.GIAP.SharedTests.Infrastructure.WireMock;
public interface IWireMockClient : IDisposable
{
    Task Stub<TDataTransferObject>(
        WireMockRequestMatch request,
        WireMockResponse<TDataTransferObject> response);
}
