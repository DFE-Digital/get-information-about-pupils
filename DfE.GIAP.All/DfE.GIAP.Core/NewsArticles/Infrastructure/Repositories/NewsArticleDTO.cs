﻿using Newtonsoft.Json;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;

public class NewsArticleDTO
{
    // TODO some of these properties should be marked nullable. See portal when articles are created. Likely DraftBody, DraftTitle
    [JsonProperty("id")]
    public string ID { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string DraftBody { get; set; }
    public string DraftTitle { get; set; }
    public bool Published { get; set; }
    public bool Archived { get; set; }
    public bool Pinned { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int DOCTYPE { get; set; } // TODO: Remove once migrated, no need for this field in the new system
}
