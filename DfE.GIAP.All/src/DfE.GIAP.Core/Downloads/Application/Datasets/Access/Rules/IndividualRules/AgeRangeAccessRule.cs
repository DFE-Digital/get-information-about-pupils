using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;

/// <summary>
/// Represents an access rule that determines download permissions based on a specified statutory age range.
/// </summary>
/// <remarks>This rule is typically used to restrict access to datasets where eligibility is defined by minimum
/// and maximum statutory age boundaries. The rule evaluates whether the provided age range in the authorisation context
/// falls within the configured limits.</remarks>
internal sealed class AgeRangeAccessRule : IDatasetAccessRule
{
    private readonly int _minLow, _maxLow, _minHigh, _maxHigh;

    public AgeRangeAccessRule(int minLow, int maxLow, int minHigh, int maxHigh)
    {
        _minLow = minLow;
        _maxLow = maxLow;
        _minHigh = minHigh;
        _maxHigh = maxHigh;
    }

    public bool HasAccess(IAuthorisationContext context)
    {
        return context.StatutoryAgeLow >= _minLow &&
               context.StatutoryAgeLow <= _maxLow &&
               context.StatutoryAgeHigh >= _minHigh &&
               context.StatutoryAgeHigh <= _maxHigh &&
               context.StatutoryAgeLow < context.StatutoryAgeHigh;
    }
}
