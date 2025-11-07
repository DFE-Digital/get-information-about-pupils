namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Client;
public interface IWireMockStubClient
{
    Task Stub<TDataTransferObject>(
        RequestMatch request,
        Response<TDataTransferObject> response);
}
