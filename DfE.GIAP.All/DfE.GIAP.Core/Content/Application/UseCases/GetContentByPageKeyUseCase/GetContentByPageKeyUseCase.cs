using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.Options;
using DfE.GIAP.Core.Content.Application.Options.Extensions;
using DfE.GIAP.Core.Content.Application.Repository;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Content.Application.UseCases.GetMultipleContentByKeys;
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
        IEnumerable<PageContentOption> contentOptions =
            _pageContentOptions.Value.TryGetPageContentOptionWithPageKey(request.PageKey);

        IEnumerable<ContentKey> uniqueContentKeys =
            contentOptions.Select((t) => ContentKey.Create(t.Key))
                .Distinct();

        List<ContentResultItem> contentItems = [];

        foreach (ContentKey contentKey in uniqueContentKeys)
        {
            Model.Content? content = await _contentReadOnlyRepository.GetContentByKeyAsync(contentKey);
            ContentResultItem item = new(contentKey.Value, content);
            contentItems.Add(item);
        }

        return new(contentItems);
    }
}

