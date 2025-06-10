namespace DfE.GIAP.Core.NewsArticles.Application.Models;

public record NewsArticle
{
    // TODO immuteable properties will break tests some are being generated via Bogus and altering model via properties
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required string DraftBody { get; init; }
    public required string DraftTitle { get; init; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool Published { get; set; }
    public bool Archived { get; set; }
    public bool Pinned { get; set; }

    public static NewsArticle Create(string title, string body, bool published, bool archived, bool pinned)
    {
        return new NewsArticle
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Body = body,
            DraftBody = string.Empty,
            DraftTitle = string.Empty,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            Published = published,
            Archived = archived,
            Pinned = pinned
        };
    }
}
