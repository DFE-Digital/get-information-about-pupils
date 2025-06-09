using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.UseCases.GetContentById;

namespace DfE.GIAP.Core.Content.Application.Repository;
public interface IContentReadOnlyRepository
{
    Task<Model.Content?> GetContentByKeyAsync(ContentKey contentKey, CancellationToken ctx = default);
}
