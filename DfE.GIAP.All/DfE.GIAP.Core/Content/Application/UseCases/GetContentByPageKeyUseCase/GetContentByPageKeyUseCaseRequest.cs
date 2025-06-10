using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Content.Application.Model;

namespace DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;
public record GetContentByPageKeyUseCaseRequest : IUseCaseRequest<GetContentByPageKeyUseCaseResponse>
{

    public GetContentByPageKeyUseCaseRequest(string pageKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pageKey);
        PageKey = pageKey;
    }
    public string PageKey { get; }
}
