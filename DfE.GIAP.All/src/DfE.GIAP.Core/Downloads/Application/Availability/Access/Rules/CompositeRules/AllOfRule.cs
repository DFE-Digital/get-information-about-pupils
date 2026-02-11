using DfE.GIAP.Core.Downloads.Application.Availability.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Availability.Access.Rules;

namespace DfE.GIAP.Core.Downloads.Application.Availability.Access.Rules.CompositeRules;

/// <summary>
/// Represents a dataset access rule that requires all contained rules to be satisfied for access to be granted.
/// </summary>
/// <remarks>Use this rule to compose multiple access rules such that access is permitted only if every individual
/// rule allows it. This is useful for enforcing stricter access control by combining several conditions. The rule
/// evaluates each contained rule in sequence and denies access if any rule is not satisfied.</remarks>
internal sealed class AllOfRule : IDatasetAccessRule
{
    private readonly IEnumerable<IDatasetAccessRule> _rules;

    public AllOfRule(params IDatasetAccessRule[] rules)
    {
        _rules = rules;
    }

    public bool HasAccess(IAuthorisationContext context)
    {
        return _rules.All(rule => rule.HasAccess(context));
    }
}
