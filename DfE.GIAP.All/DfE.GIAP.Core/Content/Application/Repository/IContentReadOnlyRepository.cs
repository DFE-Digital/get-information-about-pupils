namespace DfE.GIAP.Core.Content.Application.Repository;

/// <summary>
/// Defines a read-only repository interface for retrieving content by ID.
/// </summary>
public interface IContentReadOnlyRepository
{
    /// <summary>
    /// Asynchronously retrieves content by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the content.</param>
    /// <param name="ctx">Optional cancellation token.</param>
    /// <returns>The content associated with the specified ID.</returns>
    Task<Model.Content> GetContentByIdAsync(string id, CancellationToken ctx = default);
}
