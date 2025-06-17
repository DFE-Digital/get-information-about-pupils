using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.Options;
using DfE.GIAP.Core.Contents.Application.Options.Provider;
using DfE.GIAP.Core.Contents.Application.Repositories;

namespace DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;

/// <summary>
/// Use case for retrieving content based on a page key.
/// </summary>
internal sealed class GetContentByPageKeyUseCase : IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>
{
    private readonly IPageContentOptionsProvider _pageContentOptionProvider;
    private readonly IContentReadOnlyRepository _contentReadOnlyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetContentByPageKeyUseCase"/> class.
    /// </summary>
    /// <param name="pageContentOptionProvider">Provider for page content options.</param>
    /// <param name="contentReadOnlyRepository">Repository for retrieving content.</param>
    public GetContentByPageKeyUseCase(
        IPageContentOptionsProvider pageContentOptionProvider,
        IContentReadOnlyRepository contentReadOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(pageContentOptionProvider);
        ArgumentNullException.ThrowIfNull(contentReadOnlyRepository);
        _pageContentOptionProvider = pageContentOptionProvider;
        _contentReadOnlyRepository = contentReadOnlyRepository;
    }

    /// <summary>
    /// Handles the request to retrieve content by page key.
    /// </summary>
    /// <param name="request">The request containing the page key.</param>
    /// <returns>A response containing the retrieved content.</returns>
    public async Task<GetContentByPageKeyUseCaseResponse> HandleRequestAsync(GetContentByPageKeyUseCaseRequest request)
    {
        PageContentOption contentOptions = _pageContentOptionProvider.GetPageContentOptionWithPageKey(request.PageKey);
        ContentKey key = ContentKey.Create(contentOptions.DocumentId);
        Content? content = await _contentReadOnlyRepository.GetContentByIdAsync(key);
        return new(content);
    }
}
