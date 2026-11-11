using DfE.GIAP.Web.Helpers;

namespace DfE.GIAP.Web.Tests.Helpers;

public class PageTitleFormatterTests
{
    [Theory]
    [InlineData("", false, true, "Get information about pupils - GOV.UK")]
    [InlineData("Search", true, false, "Search - Get information about pupils - GOV.UK")]
    [InlineData("Search", false, false, "Error: Search - Get information about pupils - GOV.UK")]
    [InlineData("Error: Search", false, false, "Error: Search - Get information about pupils - GOV.UK")]
    [InlineData("Search", true, true, "Results: Search - Get information about pupils - GOV.UK")]
    [InlineData("Results: Search", true, true, "Results: Search - Get information about pupils - GOV.UK")]
    [InlineData("Search", false, true, "Error: Search - Get information about pupils - GOV.UK")]
    [InlineData("Error: Search", false, true, "Results: Error: Search - Get information about pupils - GOV.UK")]
    public void Format_uses_page_title_validation_state_and_request_method(
        string pageTitle, bool isModelStateValid, bool isPost, string expected)
    {
        Assert.Equal(expected, PageTitleFormatter.Format(pageTitle, isModelStateValid, isPost));
    }

    [Fact]
    public void Format_uses_service_title_when_page_title_is_null()
    {
        Assert.Equal("Get information about pupils - GOV.UK", PageTitleFormatter.Format(null, true, false));
    }
}
