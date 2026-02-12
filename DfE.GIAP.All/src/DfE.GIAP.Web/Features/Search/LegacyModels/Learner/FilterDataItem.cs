using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.Features.Search.LegacyModels.Learner;

[ExcludeFromCodeCoverage]
public class FilterDataItem
{
    public string Value { get; set; }
    public long? Count { get; set; }
}
