using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.Options;
using DfE.GIAP.Core.Content.Application.Options.Provider;
using DfE.GIAP.Core.Content.Application.Repository;

namespace DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;
internal sealed class GetContentByPageKeyUseCase : IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>
{
    private readonly IPageContentOptionsProvider _pageContentOptionProvider;
    private readonly IContentReadOnlyRepository _contentReadOnlyRepository;

    public GetContentByPageKeyUseCase(
        IPageContentOptionsProvider pageContentOptionProvider,
        IContentReadOnlyRepository contentReadOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(pageContentOptionProvider);
        ArgumentNullException.ThrowIfNull(contentReadOnlyRepository);
        _pageContentOptionProvider = pageContentOptionProvider;
        _contentReadOnlyRepository = contentReadOnlyRepository;
    }

    public async Task<GetContentByPageKeyUseCaseResponse> HandleRequest(GetContentByPageKeyUseCaseRequest request)
    {
        PageContentOption contentOptions = _pageContentOptionProvider.GetPageContentOptionWithPageKey(request.PageKey);
        ContentKey key = ContentKey.Create(contentOptions.DocumentId);
        Model.Content? content = await _contentReadOnlyRepository.GetContentByKeyAsync(key);
        return new(content);
    }
}
