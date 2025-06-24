using Newtonsoft.Json;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;

public record NewsArticleDto
{
    public NewsArticleDto(string id, string title, string body, string draftBody, string draftTitle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
        Title = title ?? string.Empty;
        Body = body ?? string.Empty;
        DraftTitle = draftTitle ?? string.Empty;
        DraftBody = draftBody ?? string.Empty;
        DOCTYPE = 7;
    }

    // TODO some of these properties should be marked nullable. See portal when articles are created. Likely DraftBody, DraftTitle
    [JsonProperty("id")]
    public string Id { get; }
    public string Title { get; }
    public string Body { get; }
    public string DraftBody { get; }
    public string DraftTitle { get; }
    public bool Published { get; set; }
    public bool Archived { get; set; }
    public bool Pinned { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int DOCTYPE { get; } // TODO: Remove once migrated, no need for this field in the new system
}
