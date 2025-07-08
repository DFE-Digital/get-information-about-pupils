using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Search.Learner
{
    [ExcludeFromCodeCoverage]
    public class FilterData
    {
        public string Name { get; set; }
        public List<FilterDataItem> Items { get; set; } = new List<FilterDataItem>();
    }
}
