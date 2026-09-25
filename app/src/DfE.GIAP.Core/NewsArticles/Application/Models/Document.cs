using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.NewsArticles.Application.Models;

[ExcludeFromCodeCoverage]
public class Document
{
    public string? DocumentId { get; set; }
    public string? DocumentName { get; set; }
}
