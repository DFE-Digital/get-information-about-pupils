using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Search.Learner
{
    [ExcludeFromCodeCoverage]
    public class FilterDataItem
    {
        public string Value { get; set; }
        public long? Count { get; set; }
    }
}
