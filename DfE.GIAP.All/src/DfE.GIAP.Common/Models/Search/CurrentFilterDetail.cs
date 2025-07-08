using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Search
{
    [ExcludeFromCodeCoverage]
    public class CurrentFilterDetail
    {
        public string FilterName { get; set; }

        public FilterType FilterType { get; set; }
    }
}
