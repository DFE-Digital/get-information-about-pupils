using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;


/// <summary>
/// Represents a request to retrieve content using a page key.
/// </summary>
public record GetContentByPageKeyUseCaseRequest : IUseCaseRequest<GetContentByPageKeyUseCaseResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetContentByPageKeyUseCaseRequest"/> class.
    /// </summary>
    /// <param name="pageKey">The key identifying the page.</param>
    public GetContentByPageKeyUseCaseRequest(string pageKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pageKey);
        PageKey = pageKey;
    }

    /// <summary>
    /// Gets the page key used to identify the content.
    /// </summary>
    public string PageKey { get; }
}
