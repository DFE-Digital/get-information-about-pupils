namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Request;
public record HttpMappingFile
{
    public HttpMappingFile(string? clientId, string fileName)
    {
        ClientId = clientId?.Trim() ?? Guid.NewGuid().ToString();

        Guard.ThrowIfNullOrEmpty(fileName, nameof(fileName));
        FileName = fileName;
    }

    public string ClientId { get; }
    public string FileName { get; }
}
