using DfE.GIAP.Core.Content.Application.Model;

namespace DfE.GIAP.Core.Content.Infrastructure.Repositories.Options.Extensions;
internal static class ContentRepositoryOptionsExtensions
{
    public static string GetContentDocumentIdentifierFromKey(
        this ContentRepositoryOptions contentRepositoryOptions, ContentKey contentKey) =>
            contentRepositoryOptions.ContentKeyToDocumentMapping.TryGetValue(contentKey.Value, out ContentOptions? contentReference) ?
                contentReference.DocumentId :
                    string.Empty;
}
