using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DfE.GIAP.Domain.Search.Learner;

[ExcludeFromCodeCoverage]
public class PaginatedResponse
{
    public List<Learner> Learners { get; set; } = new();
    public List<FilterData> Filters { get; set; } = new();
    public int? Count { get; set; }
}
