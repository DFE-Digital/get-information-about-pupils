using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.Features.Search.LegacyModels.Learner;

[ExcludeFromCodeCoverage]
public class FilterData
{
    public string Name { get; set; }
    public List<FilterDataItem> Items { get; set; } = new List<FilterDataItem>();
}
