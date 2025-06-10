using DfE.GIAP.Core.Content.Application.Model;

namespace DfE.GIAP.Core.Content.Application.Repository;
public interface IContentReadOnlyRepository
{
    Task<Model.Content> GetContentByKeyAsync(ContentKey contentKey, CancellationToken ctx = default);
}
