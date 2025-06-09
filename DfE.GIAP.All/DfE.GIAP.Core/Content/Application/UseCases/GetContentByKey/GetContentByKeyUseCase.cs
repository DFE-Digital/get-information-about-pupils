using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Content.Application.Repository;
using DfE.GIAP.Core.Content.Application.UseCases.GetContentByKey;

namespace DfE.GIAP.Core.Content.Application.UseCases.GetContentById;
internal sealed class GetContentByKeyUseCase : IUseCase<GetContentByKeyUseCaseRequest, GetContentByKeyUseCaseResponse>
{
    private readonly IContentReadOnlyRepository _contentReadOnlyRepository;

    public GetContentByKeyUseCase(IContentReadOnlyRepository contentReadOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(contentReadOnlyRepository);
        _contentReadOnlyRepository = contentReadOnlyRepository;
    }

    public async Task<GetContentByKeyUseCaseResponse> HandleRequest(GetContentByKeyUseCaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Model.Content? content = await _contentReadOnlyRepository.GetContentByKeyAsync(request.Key);
        return new(content);
    }
}
