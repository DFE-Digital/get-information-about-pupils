using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Content.Application.Repository;

namespace DfE.GIAP.Core.Content.Application.UseCases.GetMultipleContentByKeys;
internal sealed class GetMultipleContentByKeysUseCase : IUseCase<GetMultipleContentByKeysUseCaseRequest, GetMultipleContentByKeysUseCaseResponse>
{
    private readonly IContentReadOnlyRepository _contentReadOnlyRepository;

    public GetMultipleContentByKeysUseCase(IContentReadOnlyRepository contentReadOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(contentReadOnlyRepository);
        _contentReadOnlyRepository = contentReadOnlyRepository;
    }

    public async Task<GetMultipleContentByKeysUseCaseResponse> HandleRequest(GetMultipleContentByKeysUseCaseRequest request)
    {
        var getContentFromContentKeysTasks = request.ContentKeys
            .Distinct()
            .Select(async key => new
            {
                Key = key,
                Content = await _contentReadOnlyRepository.GetContentByKeyAsync(key)
            });

        IEnumerable<ContentItem> results =
            (await Task.WhenAll(getContentFromContentKeysTasks))
                .Select((t) =>
                    new ContentItem(Key:
                        t.Key.Value,
                        Content: t.Content));

        return new(results);
    }
}

