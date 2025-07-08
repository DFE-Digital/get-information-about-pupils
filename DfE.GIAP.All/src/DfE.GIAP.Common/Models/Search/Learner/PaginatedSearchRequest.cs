using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Search.Learner
{
    [ExcludeFromCodeCoverage]
    public class PaginatedSearchRequest
    {
        public string SearchText { get; set; }
        public Dictionary<string, string[]> Filters { get; set; }

        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public string SortField { get; set; }
        public string SortDirection { get; set; }
    }
}
