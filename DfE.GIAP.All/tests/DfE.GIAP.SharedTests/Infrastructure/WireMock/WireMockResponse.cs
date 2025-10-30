using System.Net;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock;
public class WireMockResponse<TDataTransferObject>
{
    public WireMockResponse(HttpStatusCode code, TDataTransferObject dto)
    {
        StatusCode = (int)code;
        Body = dto;
    }

    public int StatusCode { get; }
    public TDataTransferObject Body { get; }
}
