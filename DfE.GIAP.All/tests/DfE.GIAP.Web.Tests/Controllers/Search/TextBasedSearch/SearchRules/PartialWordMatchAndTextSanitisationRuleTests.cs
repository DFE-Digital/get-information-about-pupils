using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.Options;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.SearchRules;
using DfE.GIAP.Web.Features.Search.Shared.SearchRules;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.SearchRules;

public class PartialWordMatchAndTextSanitisationRuleTests
{
    [Fact]
    public void ApplySearchRules_WithMatchingRule_ReplacesHyphensAndAppliesPartialMatch()
    {
        // Arrange
        const string input = "hello-world-search";
        const string expectedSanitised = "hello world search";

        SearchRuleOptions options = new() { SearchRule = "PartialWordMatchAndTextSanitisation" };

        PartialWordMatchRule partialMatchRule = new(options);

        PartialWordMatchAndTextSanitisationRule rule = new(options, partialMatchRule);

        // Act
        string result = rule.ApplySearchRules(input);

        // Assert
        Assert.Equal(expectedSanitised, result);
    }

    [Fact]
    public void ApplySearchRules_WithNonMatchingRule_ReturnsOriginalInput()
    {
        // Arrange
        const string input = "hello-world-search";
        SearchRuleOptions options = new() { SearchRule = "SomeOtherRule" };

        PartialWordMatchRule partialMatchRule = new(options);

        PartialWordMatchAndTextSanitisationRule rule = new(options, partialMatchRule);

        // Act
        string result = rule.ApplySearchRules(input);

        // Assert
        Assert.Equal(input, result);
    }
}
