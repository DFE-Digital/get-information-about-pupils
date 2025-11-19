using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.CompositeRules;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

/// <summary>
/// Provides predefined access policies based on rules for various educational datasets.
/// </summary>
/// <remarks>This static class exposes factory methods for obtaining access policies tailored to specific datasets,
/// such as EYFSP, KS1, KS2, KS4, Phonics, Mtc, Pupil Premium, Special Education Needs, and school census datasets. Each
/// method returns an access rule that encapsulates the logic for determining user access based on roles or age ranges,
/// as appropriate for the dataset. These rules are intended to be used by authorization components to enforce
/// dataset-specific access policies.</remarks>
internal static class DatasetAccessPolicies
{
    public static IDatasetAccessRule EYFSP() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new IsLocalAuthorityUserAccessRule(),
        new AnyAgeAccessRule(),
        new AgeRangeAccessRule(2, 10, 3, 25));

    public static IDatasetAccessRule KS1() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new IsLocalAuthorityUserAccessRule(),
        new AnyAgeAccessRule(),
        new AgeRangeAccessRule(2, 13, 6, 25));

    public static IDatasetAccessRule KS2() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new IsLocalAuthorityUserAccessRule(),
        new AnyAgeAccessRule(),
        new AgeRangeAccessRule(2, 15, 6, 25));

    public static IDatasetAccessRule KS4() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new IsLocalAuthorityUserAccessRule(),
        new AnyAgeAccessRule(),
        new AgeRangeAccessRule(2, 17, 12, 25));

    public static IDatasetAccessRule Phonics() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new IsLocalAuthorityUserAccessRule(),
        new AnyAgeAccessRule(),
        new AgeRangeAccessRule(2, 10, 3, 25));

    public static IDatasetAccessRule Mtc() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new IsLocalAuthorityUserAccessRule(),
        new AnyAgeAccessRule(),
        new AgeRangeAccessRule(2, 14, 4, 25));

    public static IDatasetAccessRule PupilPremium() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new IsLocalAuthorityUserAccessRule(),
        new MinimumHighAgeAccessRule(14));

    public static IDatasetAccessRule SpecialEducationNeeds() => new AnyOfRule(
        new IsAdminUserAccessRule(),
        new IsDfEUserAccessRule(),
        new IsLocalAuthorityUserAccessRule(),
        new MinimumHighAgeAccessRule(14));

    public static IDatasetAccessRule CensusAutumn() => new AlwaysAllowAccessRule();
    public static IDatasetAccessRule CensusSpring() => new AlwaysAllowAccessRule();
    public static IDatasetAccessRule CensusSummer() => new AlwaysAllowAccessRule();
}
