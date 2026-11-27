using System.ComponentModel.DataAnnotations;

namespace DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

public class BlobStorageOptions
{
    public const string SectionName = "BlobStorageOptions";

    [Required]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    public string AccountKey { get; set; } = string.Empty;

    [Required]
    public string ContainerName { get; set; } = string.Empty;

    [Required]
    public string EndpointSuffix { get; set; } = string.Empty;

    public string ConnectionString =>
        $"AccountName={AccountName};" +
        $"AccountKey={AccountKey};" +
        $"EndpointSuffix={EndpointSuffix};" +
        $"DefaultEndpointsProtocol=https;";
}
