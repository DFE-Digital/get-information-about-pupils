using Newtonsoft.Json;

namespace DfE.GIAP.Core.Search.Application.Options.Search;

public sealed class SearchCriteriaOptions
{
    public string SearchIndex { get; set; } = string.Empty;
    /// <summary>
    /// The Azure Search mode
    /// <a href="https://learn.microsoft.com/en-us/dotnet/api/azure.search.documents.models.searchmode?view=azure-dotnet">
    /// see documentation for details</a>
    /// </summary>
    public int SearchMode { get; set; }
    public List<string>? SearchFields { get; set; }
    public int Size { get; set; }
    public bool IncludeTotalCount { get; set; }
    public List<string>? Facets { get; set; }
}
