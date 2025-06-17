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

    public HashSet<string> GetLearnerNumbers()
    {
        HashSet<string> learnerNumbers = new();
        foreach (Learner learner in Learners)
        {
            learnerNumbers.Add(learner.LearnerNumber);
        }

        return learnerNumbers;
    }

    public HashSet<string> GetLearnerNumberIds()
    {
        HashSet<string> learnerNumberIds = new();
        foreach (Learner learner in Learners)
        {
            learnerNumberIds.Add(learner.LearnerNumberId);
        }

        return learnerNumberIds;
    }

    public string GetLearnerIdsAsString()
    {
        return string.Join(",", Learners.Select(l => l.Id));
    }
}
