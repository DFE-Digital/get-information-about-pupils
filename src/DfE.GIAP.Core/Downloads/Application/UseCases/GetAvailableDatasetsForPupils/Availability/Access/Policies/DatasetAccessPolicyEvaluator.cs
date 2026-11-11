using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Access.Rules;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Access.Policies;

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
