using System.Text;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.Options;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.SearchRules;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.SearchRules;

/// <summary>
/// Facilitates search rules to be specified when running a search
/// </summary>
public class PartialWordMatchAndTextSanitisationRule : ISearchRule
{
    private readonly SearchRuleOptions _ruleOptions;
    private readonly PartialWordMatchRule _partialWordMatchRule;

    /// <summary>
    /// Construct the saerch rules provider, injecting into it the <see cref="SearchRuleOptions"/> to be applied
    /// </summary>
    /// <param name="searchRuleOptions"></param>
    public PartialWordMatchAndTextSanitisationRule(
        SearchRuleOptions searchRuleOptions,
        PartialWordMatchRule partialWordMatchRule)
    {
        _ruleOptions = searchRuleOptions;
        _partialWordMatchRule = partialWordMatchRule;
    }

    /// <summary>
    /// Apply search rules as specified in the options
    /// </summary>
    /// <param name="searchKeyword"></param>
    /// <returns></returns>
    public string ApplySearchRules(string searchKeyword) =>
        (_ruleOptions.SearchRule == "PartialWordMatchAndTextSanitisation") ? 
            _partialWordMatchRule.ApplySearchRules(
                new StringBuilder(searchKeyword.TrimEnd())
                .Replace("-", " ")
                .ToString()) :
            searchKeyword;
}
