namespace DfE.GIAP.Core.Content.Infrastructure.Repositories.Options;
public sealed class ContentRepositoryOptions
{
    public Dictionary<string, ContentOptions> ContentKeyToDocumentMapping { get; set; } = [];
}
