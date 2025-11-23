namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Request;
public record HttpMappingRequest
{
    public HttpMappingRequest(IEnumerable<HttpMappingFile> httpMappingFiles)
    {
        Guard.ThrowIfNullOrEmpty(httpMappingFiles, nameof(httpMappingFiles));

        // Check for duplicates
        IEnumerable<IGrouping<string, HttpMappingFile>> duplicateIds =
            httpMappingFiles
                .GroupBy((mapping) => mapping.Key)
                .Where(grouping => grouping.Count() > 1);

        if (duplicateIds.Any())
        {
            throw new ArgumentException($"Duplicate identifers for mappings detected: {string.Join(", ", duplicateIds)}");
        }

        Files = httpMappingFiles.ToList().AsReadOnly(); 
    }

    public IReadOnlyList<HttpMappingFile> Files { get; }

    public static HttpMappingRequest Create(IEnumerable<HttpMappingFile> httpMappingFiles) => new(httpMappingFiles);
}
