using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Policies;

public sealed class DatasetAccessPoliciesTests
{
    [Theory]
    [InlineData("EYFSP", 3, 5, true)]
    [InlineData("KS1", 4, 7, true)]
    [InlineData("KS2", 5, 11, true)]
    [InlineData("KS4", 10, 15, true)]
    [InlineData("Phonics", 4, 6, true)]
    [InlineData("Mtc", 5, 9, true)]
    [InlineData("PupilPremium", 0, 14, true)]
    [InlineData("SpecialEducationNeeds", 0, 13, false)] // below threshold
    public void CanDownload_ReturnsExpectedResult_ForAgeRangeRules(string ruleName, int low, int high, bool expected)
    {
        // Arrange
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            statutoryAgeLow: low,
            statutoryAgeHigh: high);

        IDatasetAccessRule rule = ruleName switch
        {
            "EYFSP" => DatasetAccessPolicies.EYFSP(),
            "KS1" => DatasetAccessPolicies.KS1(),
            "KS2" => DatasetAccessPolicies.KS2(),
            "KS4" => DatasetAccessPolicies.KS4(),
            "Phonics" => DatasetAccessPolicies.Phonics(),
            "Mtc" => DatasetAccessPolicies.Mtc(),
            "PupilPremium" => DatasetAccessPolicies.PupilPremium(),
            "SpecialEducationNeeds" => DatasetAccessPolicies.SpecialEducationNeeds(),
            _ => throw new ArgumentOutOfRangeException(nameof(ruleName))
        };

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("CensusAutumn")]
    [InlineData("CensusSpring")]
    [InlineData("CensusSummer")]
    public void CanDownload_AlwaysReturnsTrue_ForCensusRules(string ruleName)
    {
        // Arrange
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            statutoryAgeLow: 0,
            statutoryAgeHigh: 0);

        IDatasetAccessRule rule = ruleName switch
        {
            "CensusAutumn" => DatasetAccessPolicies.CensusAutumn(),
            "CensusSpring" => DatasetAccessPolicies.CensusSpring(),
            "CensusSummer" => DatasetAccessPolicies.CensusSummer(),
            _ => throw new ArgumentOutOfRangeException(nameof(ruleName))
        };

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("EYFSP")]
    [InlineData("KS1")]
    [InlineData("KS2")]
    [InlineData("KS4")]
    [InlineData("Phonics")]
    [InlineData("Mtc")]
    [InlineData("PupilPremium")]
    [InlineData("SpecialEducationNeeds")]
    public void CanDownload_ReturnsTrue_WhenUserIsAdmin(string ruleName)
    {
        // Arrange
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            isAdminUser: true);

        IDatasetAccessRule rule = ruleName switch
        {
            "EYFSP" => DatasetAccessPolicies.EYFSP(),
            "KS1" => DatasetAccessPolicies.KS1(),
            "KS2" => DatasetAccessPolicies.KS2(),
            "KS4" => DatasetAccessPolicies.KS4(),
            "Phonics" => DatasetAccessPolicies.Phonics(),
            "Mtc" => DatasetAccessPolicies.Mtc(),
            "PupilPremium" => DatasetAccessPolicies.PupilPremium(),
            "SpecialEducationNeeds" => DatasetAccessPolicies.SpecialEducationNeeds(),
            _ => throw new ArgumentOutOfRangeException(nameof(ruleName))
        };

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("EYFSP")]
    [InlineData("KS1")]
    [InlineData("KS2")]
    [InlineData("KS4")]
    [InlineData("Phonics")]
    [InlineData("Mtc")]
    [InlineData("PupilPremium")]
    [InlineData("SpecialEducationNeeds")]
    public void CanDownload_ReturnsTrue_WhenUserIsDfE(string ruleName)
    {
        // Arrange
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
           isDfeUser: true);

        IDatasetAccessRule rule = ruleName switch
        {
            "EYFSP" => DatasetAccessPolicies.EYFSP(),
            "KS1" => DatasetAccessPolicies.KS1(),
            "KS2" => DatasetAccessPolicies.KS2(),
            "KS4" => DatasetAccessPolicies.KS4(),
            "Phonics" => DatasetAccessPolicies.Phonics(),
            "Mtc" => DatasetAccessPolicies.Mtc(),
            "PupilPremium" => DatasetAccessPolicies.PupilPremium(),
            "SpecialEducationNeeds" => DatasetAccessPolicies.SpecialEducationNeeds(),
            _ => throw new ArgumentOutOfRangeException(nameof(ruleName))
        };

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }
}
