using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;

/// <summary>
/// Represents an access rule that permits dataset downloads only if the user's statutory high age meets or exceeds a
/// specified minimum threshold.
/// </summary>
/// <remarks>Use this rule to enforce age-based restrictions on dataset access, ensuring that only users who
/// satisfy the minimum statutory high age requirement are authorized to download the dataset.</remarks>
internal sealed class MinimumHighAgeAccessRule : IDatasetAccessRule
{
    private readonly int _threshold;

    public MinimumHighAgeAccessRule(int threshold) => _threshold = threshold;

    public bool HasAccess(IAuthorisationContext context) =>
        context.StatutoryAgeHigh >= _threshold;
}
