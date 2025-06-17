
namespace DfE.GIAP.Core.NewsArticles.Application.Models;

public record NewsArticle
{
    // TODO immutable properties will break tests some are being generated via Bogus and altering model via properties
    public required NewsArticleIdentifier Id { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required string DraftBody { get; init; }
    public required string DraftTitle { get; init; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool Published { get; set; }
    public bool Archived { get; set; }
    public bool Pinned { get; set; }
}
