namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Client.Java.DataTransferObjects;
public record WireMockJavaMappingRequestResponseModel
{
    public int status { get; set; }
    public IDictionary<string, object>? headers { get; set; }
    public object? jsonBody { get; set; }
}
