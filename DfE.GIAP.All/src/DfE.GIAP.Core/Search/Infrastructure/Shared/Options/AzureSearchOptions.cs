using Newtonsoft.Json;

namespace DfE.GIAP.Core.Search.Infrastructure.Shared.Options;

/// <summary>
/// The search options to use by the <see cref="CognitiveSearchServiceAdapter{TSearchResult}"/>
/// which is set using the IOptions interface
/// </summary>
public class AzureSearchOptions
{
    /// <summary>
    /// Represents a collection of Search Index configurations.
    /// </summary>
    [JsonProperty(nameof(Indexes))]
    public Dictionary<string, AzureSearchIndexOptions>? Indexes { get; set; }
}


