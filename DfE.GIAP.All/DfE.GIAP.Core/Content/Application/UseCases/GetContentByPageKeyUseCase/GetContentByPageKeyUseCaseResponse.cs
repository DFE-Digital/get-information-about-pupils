using DfE.GIAP.Core.Content.Application.Model;

namespace DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;
public record ContentResultItem(string Key, Model.Content? Content);

public record GetContentByPageKeyUseCaseResponse(IEnumerable<ContentResultItem> ContentResultItems);
