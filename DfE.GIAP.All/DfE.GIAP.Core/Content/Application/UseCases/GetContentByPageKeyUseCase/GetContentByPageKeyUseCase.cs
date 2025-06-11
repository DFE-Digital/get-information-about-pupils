using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.Options;
using DfE.GIAP.Core.Content.Application.Options.Extensions;
using DfE.GIAP.Core.Content.Application.Repository;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;
internal sealed class GetContentByPageKeyUseCase : IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>
{
    private readonly IContentReadOnlyRepository _contentReadOnlyRepository;
    private readonly IOptions<PageContentOptions> _pageContentOptions;

    public GetContentByPageKeyUseCase(
        IContentReadOnlyRepository contentReadOnlyRepository,
        IOptions<PageContentOptions> pageContentOptions)
    {
        ArgumentNullException.ThrowIfNull(contentReadOnlyRepository);
        _contentReadOnlyRepository = contentReadOnlyRepository;
        _pageContentOptions = pageContentOptions;
    }

    public async Task<GetContentByPageKeyUseCaseResponse> HandleRequest(GetContentByPageKeyUseCaseRequest request)
    {
        PageContentOption contentOptions =
            _pageContentOptions.Value.GetPageContentOptionWithPageKey(request.PageKey);
        ContentKey key = ContentKey.Create(request.PageKey);
        Model.Content? content = await _contentReadOnlyRepository.GetContentByKeyAsync(key);
        return new(content);
    }
}

