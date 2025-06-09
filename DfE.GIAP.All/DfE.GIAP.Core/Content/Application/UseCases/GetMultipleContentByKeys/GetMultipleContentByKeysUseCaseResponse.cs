using DfE.GIAP.Core.Content.Application.Model;

namespace DfE.GIAP.Core.Content.Application.UseCases.GetMultipleContentByKeys;
public record ContentItem(string Key, Model.Content? Content);

public record GetMultipleContentByKeysUseCaseResponse(IEnumerable<ContentItem> Items);
