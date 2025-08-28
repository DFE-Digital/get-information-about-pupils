namespace DfE.GIAP.Core.Search.Infrastructure.Options;

/// <summary>
/// The search options to use by the <see cref="CognitiveSearchServiceAdapter{TSearchResult}"/>
/// which is set using the IOptions interface
/// </summary>
public class AzureSearchOptions
{
    /// <summary>
    /// The Azure AI Search index used to target for search requests.
    /// </summary>
    public string SearchIndex { get; set; } = string.Empty;
    /// <summary>
    /// The Azure Search mode <a href="https://learn.microsoft.com/en-us/dotnet/api/azure.search.documents.models.searchmode?view=azure-dotnet">
    /// see documentation for details</a>
    /// </summary>
    public int SearchMode { get; set; }
    /// <summary>
    /// The number of search results returned.
    /// </summary>
    public int Size { get; set; }
    /// <summary>
    /// The number of search results returned.
    /// </summary>
    public bool IncludeTotalCount { get; set; }
}
