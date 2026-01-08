using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Policies;

public sealed class DatasetAccessPolicyEvaluatorTests
{
    [Fact]
    public void HasAccess_ReturnsTrue_WhenRuleReturnsTrue()
    {
        DatasetAccessPolicyEvaluator evaluator = new(new Dictionary<Dataset, IDatasetAccessRule>
        {
            [Dataset.KS1] = DatasetAccessRuleTestDouble.ReturnsTrue()
        });

        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        bool response = evaluator.HasAccess(context, Dataset.KS1);

        Assert.True(response);
    }

    [Fact]
    public void HasAccess_ReturnsFalse_WhenRuleReturnsFalse()
    {
        DatasetAccessPolicyEvaluator evaluator = new(new Dictionary<Dataset, IDatasetAccessRule>
        {
            [Dataset.KS1] = DatasetAccessRuleTestDouble.ReturnsFalse()
        });

        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        bool response = evaluator.HasAccess(context, Dataset.KS1);
        Assert.False(response);
    }

    [Fact]
    public void HasAccess_ReturnsFalse_WhenDatasetIsNotRegistered()
    {
        DatasetAccessPolicyEvaluator evaluator = new(new Dictionary<Dataset, IDatasetAccessRule>
        {
            [Dataset.KS1] = DatasetAccessRuleTestDouble.ReturnsTrue()
        });

        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        bool response = evaluator.HasAccess(context, Dataset.KS2);
        Assert.False(response); // KS2 not registered
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenPoliciesIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DatasetAccessPolicyEvaluator(null!));
    }

    [Fact]
    public void HasAccess_ReturnsFalse_WhenPolicyDictionaryIsEmpty()
    {
        DatasetAccessPolicyEvaluator evaluator = new(new Dictionary<Dataset, IDatasetAccessRule>());
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();

        bool result = evaluator.HasAccess(context, Dataset.KS1);

        Assert.False(result);
    }
}
