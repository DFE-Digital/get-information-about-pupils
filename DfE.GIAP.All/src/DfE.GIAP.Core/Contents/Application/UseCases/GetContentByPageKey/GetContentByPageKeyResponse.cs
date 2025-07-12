using DfE.GIAP.Core.Contents.Application.Models;

namespace DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKey;

/// <summary>
/// Represents the response returned after retrieving content by page key.
/// </summary>
/// <param name="Content">The content associated with the requested page key.</param>
public record GetContentByPageKeyResponse(Content? Content);
