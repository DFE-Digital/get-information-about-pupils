using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.CompositeRules;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;

internal static class DatasetAccessRules
{
    public static IDatasetAccessRule EYFSP() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new AgeRangeAccessRule(2, 10, 3, 25));

    public static IDatasetAccessRule KS1() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new AgeRangeAccessRule(2, 13, 6, 25));

    public static IDatasetAccessRule KS2() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new AgeRangeAccessRule(2, 15, 6, 25));

    public static IDatasetAccessRule KS4() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new AgeRangeAccessRule(2, 17, 12, 25));

    public static IDatasetAccessRule Phonics() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new AgeRangeAccessRule(2, 10, 3, 25));

    public static IDatasetAccessRule Mtc() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new AgeRangeAccessRule(2, 14, 4, 25));

    public static IDatasetAccessRule PupilPremium() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new MinimumHighAgeRule(14));

    public static IDatasetAccessRule SpecialEducationNeeds() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new MinimumHighAgeRule(14));

    public static IDatasetAccessRule CensusAutumn() => new AlwaysAllowRule();
    public static IDatasetAccessRule CensusSpring() => new AlwaysAllowRule();
    public static IDatasetAccessRule CensusSummer() => new AlwaysAllowRule();
}
