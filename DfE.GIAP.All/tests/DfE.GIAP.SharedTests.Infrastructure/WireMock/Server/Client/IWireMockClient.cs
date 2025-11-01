namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Client;
public interface IWireMockClient
{
    Task Stub<TDataTransferObject>(
        RequestMatch request,
        Response<TDataTransferObject> response);
}
