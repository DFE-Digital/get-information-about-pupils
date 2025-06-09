using DfE.GIAP.Core.Content.Application.UseCases.GetContentById;

namespace DfE.GIAP.Core.Content.Infrastructure.Repositories.Options;
public sealed class ContentRepositoryOptions
{
    public Dictionary<string, ContentOptions> ContentKeyMappings { get; set; } = [];
}
