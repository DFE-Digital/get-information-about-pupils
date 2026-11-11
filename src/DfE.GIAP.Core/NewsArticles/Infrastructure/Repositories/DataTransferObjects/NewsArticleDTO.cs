namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;

public record NewsArticleDto
{
    // TODO some of these properties should be marked nullable. See portal when articles are created. Likely DraftBody, DraftTitle
    public required string id { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public bool Published { get; set; }
    public bool Pinned { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
