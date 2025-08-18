namespace DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
public class BlobItemInfo
{
    public string? Name { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public long? Size { get; set; }
    public string? ContentType { get; set; }
}
