namespace DfE.GIAP.Core.Content.Application.Repository;
public interface IContentReadOnlyRepository
{
    Task<Model.Content> GetContentByIdAsync(string id, CancellationToken ctx = default);
}
