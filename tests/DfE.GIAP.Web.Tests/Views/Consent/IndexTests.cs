using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DfE.GIAP.Web.Tests.Views;
using DfE.GIAP.Web.ViewModels;

namespace DfE.GIAP.Web.Tests.Views.Consent;

public sealed class IndexTests : IClassFixture<HostFixture>
{
    private readonly HostFixture _hostFixture;

    public IndexTests(HostFixture hostFixture)
    {
        _hostFixture = hostFixture;
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Index_RendersBoundCheckboxAndContinueButton(bool consentGiven, bool hasError)
    {
        IHtmlDocument document = await _hostFixture.RenderViewAsync<ConsentViewModel>(
            "/Views/Consent/Index.cshtml",
            model =>
            {
                model.ConsentGiven = consentGiven;
                model.HasError = hasError;
            });
        IElement checkbox = Assert.Single(document.QuerySelectorAll("input[type=checkbox]"));
        Assert.Equal("ConsentGiven", checkbox.GetAttribute("name"));
        Assert.Equal("true", checkbox.GetAttribute("value"));
        Assert.False(string.IsNullOrEmpty(checkbox.Id));
        Assert.Equal(consentGiven, checkbox.HasAttribute("checked"));
        IElement label = Assert.Single(document.QuerySelectorAll($"label[for='{checkbox.Id}']"));
        Assert.Equal("I confirm that I have read and understood the conditions", label.TextContent.Trim());

        IElement button = Assert.Single(document.QuerySelectorAll("button.govuk-button"));
        Assert.Equal("submit", button.GetAttribute("type"));
        Assert.Equal("Continue", button.TextContent.Trim());

        IElement? error = document.QuerySelector(".govuk-error-message");
        Assert.Equal(hasError, error is not null);
        Assert.Equal(hasError, document.QuerySelector(".govuk-form-group--error") is not null);
        if (hasError)
        {
            Assert.NotNull(error);
            IElement errorElement = error!;
            Assert.False(string.IsNullOrEmpty(errorElement.Id));
            Assert.Contains("Select if you have read and understood the conditions", errorElement.TextContent);
            Assert.Contains(errorElement.Id, checkbox.GetAttribute("aria-describedby") ?? "");
        }
    }
}
