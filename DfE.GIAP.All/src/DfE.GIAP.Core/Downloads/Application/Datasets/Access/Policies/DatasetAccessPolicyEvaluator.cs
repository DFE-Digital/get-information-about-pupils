using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

public class DatasetAccessPolicyEvaluator : IDatasetAccessEvaluator
{
    private readonly IReadOnlyDictionary<Dataset, IDatasetAccessRule> _policies;

    public DatasetAccessPolicyEvaluator(IReadOnlyDictionary<Dataset, IDatasetAccessRule> policies)
    {
        ArgumentNullException.ThrowIfNull(policies);
        _policies = policies;
    }

    public bool HasAccess(IAuthorisationContext context, Dataset dataset)
    {
        return _policies.TryGetValue(dataset, out IDatasetAccessRule? rule) && rule.HasAccess(context);
    }
}
