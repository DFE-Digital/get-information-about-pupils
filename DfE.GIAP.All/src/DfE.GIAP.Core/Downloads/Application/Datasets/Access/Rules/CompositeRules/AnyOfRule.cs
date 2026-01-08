using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.CompositeRules;

/// <summary>
/// Represents a dataset access rule that grants access if any of the specified rules allow it.
/// </summary>
/// <remarks>Use this rule to compose multiple dataset access rules, granting access when at least one underlying
/// rule permits it. This is useful for scenarios where access should be allowed if any of several conditions are
/// met.</remarks>
internal sealed class AnyOfRule : IDatasetAccessRule
{
    private readonly IEnumerable<IDatasetAccessRule> _rules;

    public AnyOfRule(params IDatasetAccessRule[] rules)
    {
        _rules = rules;
    }

    public bool HasAccess(IAuthorisationContext context)
    {
        return _rules.Any(rule => rule.HasAccess(context));
    }
}
