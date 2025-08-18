namespace DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

public class BlobStorageOptions
{
    public string? AccountName { get; set; }
    public string? AccountKey { get; set; }
    public string? ContainerName { get; set; }
    public string? EndpointSuffix { get; set; } = "core.windows.net"; // Optional override
}
