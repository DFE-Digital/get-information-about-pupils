using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Search
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class SearchFilters
    {
        public SearchFilters()
        {
            this.CustomFilterText = new CustomFilterText();
            this.CurrentFiltersApplied = new List<CurrentFilterDetail>();
        }

        public CustomFilterText CustomFilterText { get; set; }

        public List<CurrentFilterDetail> CurrentFiltersApplied { get; set; }

        public string CurrentFiltersAppliedString { get; set; } = string.Empty;
    }
}
