using DfE.GIAP.Core.Downloads.Application.Availability.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Availability.Access.Rules.IndividualRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class AnyAgeAccessRuleTests
{
    [Fact]
    public void CanDownload_ReturnsTrue_WhenUserIsAnyAge()
    {
        // Arrange
        AnyAgeAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            AnyAgeUser: true);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_WhenUserIsNotAnyAge()
    {
        // Arrange
        AnyAgeAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            AnyAgeUser: false);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.False(result);
    }
}
