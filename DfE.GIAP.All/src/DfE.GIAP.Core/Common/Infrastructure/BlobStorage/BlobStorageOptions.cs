namespace DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

public class BlobStorageOptions
{
    public const string SectionName = "BlobStorageOptions";
    public string AccountName { get; set; } = string.Empty;
    public string AccountKey { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string EndpointSuffix { get; set; } = string.Empty;
}
