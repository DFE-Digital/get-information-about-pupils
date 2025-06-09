using DfE.GIAP.Core.Content.Application.Model;

namespace DfE.GIAP.Core.Content.Infrastructure.Repositories.Options.Extensions;
internal static class ContentRepositoryOptionsExtensions
{
    public static string GetContentIdentifierFromKey(
        this ContentRepositoryOptions contentRepositoryOptions, ContentKey contentKey) =>
            contentRepositoryOptions.ContentKeyMappings.TryGetValue(contentKey.Value, out ContentOptions? contentReference) ?
                contentReference.Id :
                    string.Empty;
}
