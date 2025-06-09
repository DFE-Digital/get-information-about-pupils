using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Content.Application.Model;

namespace DfE.GIAP.Core.Content.Application.UseCases.GetContentByKey;
public record GetContentByKeyUseCaseRequest : IUseCaseRequest<GetContentByKeyUseCaseResponse>
{
    public GetContentByKeyUseCaseRequest(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        Key = ContentKey.Create(key);
    }

    public ContentKey Key { get; }
}
