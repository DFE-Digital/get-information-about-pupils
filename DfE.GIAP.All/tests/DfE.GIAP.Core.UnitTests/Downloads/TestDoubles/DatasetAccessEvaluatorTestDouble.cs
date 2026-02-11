using DfE.GIAP.Core.Downloads.Application.Availability.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class DatasetAccessEvaluatorTestDouble
{
    public static IDatasetAccessEvaluator AllowAll() => new StubEvaluator((_, _) => true);
    public static IDatasetAccessEvaluator DenyAll() => new StubEvaluator((_, _) => false);

    public static IDatasetAccessEvaluator WithRule(Func<IAuthorisationContext, Dataset, bool> rule) =>
        new StubEvaluator(rule);

    private sealed class StubEvaluator : IDatasetAccessEvaluator
    {
        private readonly Func<IAuthorisationContext, Dataset, bool> _rule;

        public StubEvaluator(Func<IAuthorisationContext, Dataset, bool> rule)
        {
            _rule = rule;
        }

        public bool HasAccess(IAuthorisationContext context, Dataset dataset) => _rule(context, dataset);
    }
}
