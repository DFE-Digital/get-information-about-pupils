using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Content.Application.Model;

namespace DfE.GIAP.Core.Content.Application.UseCases.GetMultipleContentByKeys;
public record GetMultipleContentByKeyUseCaseRequest : IUseCaseRequest<GetMultipleContentByKeyUseCaseResponse>
{

    public GetMultipleContentByKeyUseCaseRequest(IEnumerable<string> contentKeys)
    {
        ArgumentNullException.ThrowIfNull(contentKeys);
        ContentKeys = contentKeys.Select(ContentKey.Create);
    }

    public IEnumerable<ContentKey> ContentKeys { get; }
}
