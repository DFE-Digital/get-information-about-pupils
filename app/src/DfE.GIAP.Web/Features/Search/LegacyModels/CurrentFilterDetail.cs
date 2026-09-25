using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.Features.Search.LegacyModels;

[ExcludeFromCodeCoverage]
public class CurrentFilterDetail
{
    public string FilterName { get; set; }

    public FilterType FilterType { get; set; }
}
