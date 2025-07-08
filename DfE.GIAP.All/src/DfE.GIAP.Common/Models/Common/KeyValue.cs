using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Common
{
    [ExcludeFromCodeCoverage]
    public class KeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
