using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Search;

[ExcludeFromCodeCoverage]
[Serializable]
public class SearchFilters
{
    public SearchFilters()
    {
        CustomFilterText = new();
        CurrentFiltersApplied = new();
    }

    public CustomFilterText CustomFilterText { get; set; }

    public List<CurrentFilterDetail> CurrentFiltersApplied { get; set; }

    public string CurrentFiltersAppliedString { get; set; } = string.Empty;
}
