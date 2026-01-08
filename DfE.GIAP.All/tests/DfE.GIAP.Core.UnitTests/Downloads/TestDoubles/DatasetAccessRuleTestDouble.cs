using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class DatasetAccessRuleTestDouble
{
    public static IDatasetAccessRule ReturnsTrue() => new StubAccessRule(true);
    public static IDatasetAccessRule ReturnsFalse() => new StubAccessRule(false);

    private sealed class StubAccessRule : IDatasetAccessRule
    {
        private readonly bool _result;
        public StubAccessRule(bool result) => _result = result;
        public bool HasAccess(IAuthorisationContext context) => _result;
    }
}
