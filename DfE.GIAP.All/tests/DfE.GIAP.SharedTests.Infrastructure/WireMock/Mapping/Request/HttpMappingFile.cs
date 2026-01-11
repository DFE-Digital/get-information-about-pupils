namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Request;
public record HttpMappingFile
{
    public HttpMappingFile(string? key, string fileName)
    {
        Key = key?.Trim() ?? Guid.NewGuid().ToString();

        Guard.ThrowIfNullOrEmpty(fileName, nameof(fileName));
        FileName = fileName;
    }

    public string Key { get; }
    public string FileName { get; }
}
