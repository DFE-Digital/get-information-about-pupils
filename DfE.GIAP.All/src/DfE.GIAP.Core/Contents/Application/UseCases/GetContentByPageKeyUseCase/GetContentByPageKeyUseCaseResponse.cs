using DfE.GIAP.Core.Contents.Application.Models;

namespace DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;

/// <summary>
/// Represents the response returned after retrieving content by page key.
/// </summary>
/// <param name="Content">The content associated with the requested page key.</param>
public record GetContentByPageKeyUseCaseResponse(Content? Content);
